using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Cubes.Core.Base;
using Cubes.Core.Web.Filters;
using Cubes.Core.Web.Formatters;
using Cubes.Core.Web.ResponseWrapping;
// using Cubes.Core.Web.StaticContent;
using Cubes.Core.Web.Swager;
// using Cubes.Core.Web.UIHelpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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

            var enableCompression = configuration.GetValue<bool>(CubesConstants.Config_HostEnableCompression, true);
            if (enableCompression)
                services.AddResponseCompression();
            services.AddCubesSwaggerServices(configuration);
            services.AddSingleton<IApiResponseBuilder, ApiResponseBuilder>();

            // TODO services.AddUIServices();

            mvcBuilder.AddMvcOptions(options =>
            {
                options.Filters.Add(typeof(ValidateModelFilterAttribute));
                options.InputFormatters.Insert(0, new RawStringInputFormatter());
            });
        }

        public static IApplicationBuilder UseCubesApi(this IApplicationBuilder app,
            IConfiguration configuration,
            IWebHostEnvironment env,
            IApiResponseBuilder responseBuilder,
            ILoggerFactory loggerFactory)
        {
            var enableCompression = configuration.GetValue<bool>(CubesConstants.Config_HostEnableCompression, true);
            if (enableCompression)
                app.UseResponseCompression();

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseCustomExceptionHandler(responseBuilder, loggerFactory);

            var corsPolicies = configuration
                .GetCorsPolicies()
                .Select(p => p.PolicyName)
                .ToList();
            foreach (var policy in corsPolicies)
                app.UseCors(policy);

            return app
                // TODO .UseHomePage()
                // TODO .UseAdminPage(configuration, loggerFactory)
                .UseCubesSwagger()
                // TODO .UseStaticContent(configuration, loggerFactory)
                .UseCubesMiddleware(loggerFactory, responseBuilder)
                .UseResponseWrapper();
        }

        public static IApplicationBuilder UseCubesMiddleware(this IApplicationBuilder app,
            ILoggerFactory loggerFactory,
            IApiResponseBuilder responseBuilder)
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

                var watcher = Stopwatch.StartNew();
                ctx.Response.OnStarting(() =>
                {
                    watcher.Stop();
                    ctx.Response.Headers.Add(CUBES_HEADER_REQUEST_ID, new[] { requestID });
                    return Task.FromResult(0);
                });

                await next.Invoke();

                // Just in case...
                watcher.Stop();

                var httpContextAccessor = ctx.RequestServices.GetService<IHttpContextAccessor>();

                // Inform user
                logger.LogInformation("{IP} [{startedAt}] \"{info}\", {statusCode}, {elapsed} ms",
                    httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                    context.StartedAt,
                    context.SourceInfo,
                    ctx.Response.StatusCode,
                    watcher.ElapsedMilliseconds);
            });

            app.UseStatusCodePages(async context =>
            {
                context.HttpContext.Response.ContentType = "application/json";
                var apiResponse = responseBuilder.Create()
                            .HasErrors()
                            .WithStatusCode(context.HttpContext.Response.StatusCode)
                            .WithMessage($"Invalid request, status code: {context.HttpContext.Response.StatusCode}")
                            .WithResponse(new
                            {
                                Details = $"URL: {context.HttpContext.Request.Path}, Method: {context.HttpContext.Request.Method}"
                            });
                await context
                    .HttpContext
                    .Response
                    .WriteAsync(apiResponse.ToString());
            });

            return app;
        }

        public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder app,
            IApiResponseBuilder responseBuilder,
            ILoggerFactory loggerFactory)
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
                            .HasErrors()
                            .WithStatusCode(context.Response.StatusCode)
                            .WithMessage("An unhandled exception occurred while processing the request.")
                            .WithResponse(new
                            {
                                Details = details.ToString(),
                                RequestInfo = $"{context.Request.Path} [{context.Request.Method}]"
                            });
                        await context
                            .Response
                            .WriteAsync(apiResponse.ToString());
                    }
                });
            });
            return app;
        }

        private sealed class ErrorDetails
        {
            public int StatusCode { get; set; }
            public string Message { get; set; }
            public string Details { get; set; }
            public string RequestInfo { get; set; }

            public override string ToString() => JsonConvert.SerializeObject(this);
        }
    }
}
