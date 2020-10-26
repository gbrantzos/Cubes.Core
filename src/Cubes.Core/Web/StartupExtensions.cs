using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Cubes.Core.Base;
using Cubes.Core.Commands;
using Cubes.Core.Metrics;
using Cubes.Core.Security;
using Cubes.Core.Utilities;
using Cubes.Core.Web.Filters;
using Cubes.Core.Web.Formatters;
using Cubes.Core.Web.ResponseWrapping;
using Cubes.Core.Web.StaticContent;
using Cubes.Core.Web.Swager;
using Cubes.Core.Web.UIHelpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Prometheus;
using Cubes.Core.Web.IpRestrictions;

[assembly: SwaggerCategory("Core")]
namespace Cubes.Core.Web
{
    public static class StartupExtensions
    {
        public const string CUBES_HEADER_REQUEST_ID = "X-Cubes-RequestID";
        public const string CUBES_MIDDLEWARE_LOGGER = "Cubes.Web.CubesMiddleware";

        public static void AddCubesWeb(this IServiceCollection services, IConfiguration configuration, IMvcBuilder mvcBuilder)
        {
            services.AddHttpContextAccessor();
            services.AddCorsConfiguration(configuration);
            services.AddHealthChecks();

            var enableCompression = configuration.GetValue<bool>(CubesConstants.Config_HostEnableCompression, true);
            if (enableCompression)
                services.AddResponseCompression();
            services.AddCubesSwaggerServices(configuration);
            services.AddSingleton<IApiResponseBuilder, ApiResponseBuilder>();
            services.AddSingleton<IIpMatcher, IpMatcher>();

            services.AddUIServices();

            mvcBuilder.AddMvcOptions(options =>
            {
                options.Filters.Add(typeof(ValidateModelFilterAttribute));
                options.InputFormatters.Insert(0, new RawStringInputFormatter());
            });

            var secretKey = configuration.GetValue<string>(CubesConstants.Config_ApiKey, String.Empty);
            if (String.IsNullOrEmpty(secretKey))
                throw new Exception("You must define an API key!");
            if (secretKey.Length < 16)
                throw new ArgumentException("Api key should be at least 16 characters long");

            // https://blog.ploeh.dk/2019/11/25/timespan-configuration-values-in-net-core/
            var tokenLifetime = configuration.GetValue(CubesConstants.Config_KeyLifetime, TimeSpan.FromHours(2));

            services.AddCubesAuthentication(o =>
            {
                o.SecretKey     = secretKey;
                o.TokenLifetime = tokenLifetime;
            });

            var assemblies = AppDomain
               .CurrentDomain
               .GetAssemblies();

            var providers = assemblies
                .SelectMany(t => t.GetTypes())
                .Where(t => t.IsClass && t.GetInterfaces().Contains(typeof(IRequestSampleProvider)))
                .ToList();
            foreach (var prv in providers)
                services.AddTransient(prv);
        }

        public static IApplicationBuilder UseCubesApi(this IApplicationBuilder app,
            IConfiguration configuration,
            IWebHostEnvironment env,
            IApiResponseBuilder responseBuilder,
            ILoggerFactory loggerFactory,
            IMetrics metrics,
            JsonSerializerSettings serializerSettings)
        {
            var enableCompression = configuration.GetValue<bool>(CubesConstants.Config_HostEnableCompression, true);
            if (enableCompression)
                app.UseResponseCompression();

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseCustomExceptionHandler(responseBuilder, loggerFactory, serializerSettings);

            var corsPolicies = configuration
                .GetCorsPolicies()
                .Select(p => p.PolicyName)
                .ToList();
            foreach (var policy in corsPolicies)
                app.UseCors(policy);

            // Based on:
            // https://gunnarpeipman.com/aspnet-core-health-checks/
            // https://www.youtube.com/watch?v=bdgtYbGYsK0
            // https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks

            var options = new HealthCheckOptions();
            options.AllowCachingResponses = false;
            options.ResponseWriter = async (context, result) =>
            {
                context.Response.ContentType = MediaTypeNames.Application.Json;
                var response = new
                {
                    Status = result.Status.ToString(),
                    Checks = result.Entries.Select(e => new
                    {
                        Component = e.Key,
                        e.Value.Description,
                        e.Value.Status,
                        e.Value.Duration
                    }).ToList(),
                    Duration = result.TotalDuration
                };

                await context.Response.WriteAsync(response.AsJson());
            };
            var hcEndpoint = configuration.GetValue(CubesConstants.Config_HostHealthCheckEndpoint, "/healthcheck");
            if (!hcEndpoint.StartsWith("/"))
                hcEndpoint = "/" + hcEndpoint;
            app.UseHealthChecks(hcEndpoint, options);

            var metricsEndpoint = configuration.GetValue(CubesConstants.Config_HostMetricsEndpoint, "/metrics");
            if (!metricsEndpoint.StartsWith("/"))
                metricsEndpoint = "/" + metricsEndpoint;
            return app
                .UseHomePage()
                .UseAdminPage(configuration, loggerFactory)
                .UseCubesSwagger()
                .UseStaticContent(configuration, loggerFactory)
                .UseCubesMiddleware(loggerFactory, responseBuilder, metrics, serializerSettings)
                .UseMetricServer(metricsEndpoint)
                .UseResponseWrapper();
        }

        public static IApplicationBuilder UseCubesMiddleware(this IApplicationBuilder app,
            ILoggerFactory loggerFactory,
            IApiResponseBuilder responseBuilder,
            IMetrics metrics,
            JsonSerializerSettings serializerSettings)
        {
            app.Use(async (ctx, next) =>
            {
                // Prepare
                var logger = loggerFactory.CreateLogger(CUBES_MIDDLEWARE_LOGGER);
                var requestID = Guid.NewGuid().ToString("N");
                var requestInfo = $"{ctx.Request.Method} {ctx.Request.Path}{ctx.Request.Query.AsString()}";

                // Add to headers
                ctx.Request.Headers.Add(CUBES_HEADER_REQUEST_ID, new[] { requestID });

                // Provide context information
                var ctxProvider = ctx.RequestServices.GetService<IContextProvider>();
                var context = new Context(requestID, requestInfo);
                ctxProvider.Current = context;

                var watch = Stopwatch.StartNew();
                ctx.Response.OnStarting(() =>
                {
                    watch.Stop();
                    ctx.Response.Headers.Add(CUBES_HEADER_REQUEST_ID, new[] { requestID });
                    return Task.FromResult(0);
                });

                await next.Invoke();

                // Just in case...
                watch.Stop();

                var httpContextAccessor = ctx.RequestServices.GetService<IHttpContextAccessor>();

                // Inform user
                var level =
                    ctx.Response.StatusCode >= 400 && ctx.Response.StatusCode < 500 ? LogLevel.Warning :
                    ctx.Response.StatusCode >= 500 ? LogLevel.Error : LogLevel.Information;

                logger.Log(level, "{IP} [{startedAt}] \"{info}\", {statusCode}, {elapsed} ms",
                    httpContextAccessor.HttpContext.Connection.RemoteIpAddress,
                    context.StartedAt,
                    context.SourceInfo,
                    ctx.Response.StatusCode,
                    watch.ElapsedMilliseconds);

                // Metrics
                metrics
                    .GetCounter(CubesCoreMetrics.CubesCoreApiCallsCount)
                    .WithLabels(ctx.Request.Method, ctx.Request.Path)
                    .Inc();
                metrics
                    .GetHistogram(CubesCoreMetrics.CubesCoreApiCallsDuration)
                    .WithLabels(ctx.Request.Method, ctx.Request.Path)
                    .Observe(watch.Elapsed.TotalSeconds);
            });

            app.UseStatusCodePages(async context =>
            {
                context.HttpContext.Response.ContentType = "application/json";
                var apiResponse = responseBuilder.Create()
                    .WithStatusCode(context.HttpContext.Response.StatusCode)
                    .WithMessage($"Invalid request, status code: {context.HttpContext.Response.StatusCode}")
                    .WithData(new
                    {
                        Details = $"URL: {context.HttpContext.Request.Path}, Method: {context.HttpContext.Request.Method}"
                    });
                await context
                    .HttpContext
                    .Response
                    .WriteAsync(apiResponse.AsJson(serializerSettings));
            });

            return app;
        }

        public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder app,
            IApiResponseBuilder responseBuilder,
            ILoggerFactory loggerFactory,
            JsonSerializerSettings serializerSettings)
        {
            var logger = loggerFactory.CreateLogger("Cubes.Web.CustomExceptionHandler");
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        var ex = contextFeature.Error;

                        var frame      = new StackTrace(ex, true).GetFrame(0);
                        var details    = new StringBuilder();
                        var methodInfo = $"{frame.GetMethod().DeclaringType.FullName}.{frame.GetMethod().Name}()";
                        var fileInfo   = $"{Path.GetFileName(frame.GetFileName())}, line {frame.GetFileLineNumber()}";
                        details
                            .Append(ex.GetType().Name)
                            .Append(": ")
                            .AppendLine(ex.Message);
                        details
                            .Append(methodInfo)
                            .Append(" in ")
                            .AppendLine(fileInfo);

                        var apiResponse = responseBuilder.Create()
                            .WithStatusCode(context.Response.StatusCode)
                            .WithMessage("An unhandled exception occurred while processing the request.")
                            .WithData(new
                            {
                                Details = details.ToString(),
                                RequestInfo = $"{context.Request.Path} [{context.Request.Method}]"
                            });
                        await context
                            .Response
                            .WriteAsync(apiResponse.AsJson(serializerSettings));
                    }
                });
            });
            return app;
        }
    }
}
