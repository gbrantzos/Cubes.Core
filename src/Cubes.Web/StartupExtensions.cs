using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Cubes.Core.Base;
using Cubes.Core.Scheduling.Jobs;
using Cubes.Web.StaticContent;
using Cubes.Web.Swager;
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
namespace Cubes.Web
{
    public static class StartupExtensions
    {
        public const string CUBES_HEADER_REQUEST_ID = "X-Cubes-RequestID";
        public const string CUBES_MIDDLEWARE_LOGGER = "Cubes.Web.CubesMiddleware";

        public static void AddCubesWeb(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();

            var enableCompression = configuration.GetValue<bool>(CubesConstants.Config_HostEnableCompression, true);
            if (enableCompression)
                services.AddResponseCompression();
            services.AddCubesSwaggerServices(configuration);
        }

        public static IApplicationBuilder UseCubesApi(this IApplicationBuilder app,
            IConfiguration configuration,
            IWebHostEnvironment env,
            ILoggerFactory loggerFactory)
        {
            var enableCompression = configuration.GetValue<bool>(CubesConstants.Config_HostEnableCompression, true);
            if (enableCompression)
                app.UseResponseCompression();

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseCustomExceptionHandler(loggerFactory);

            return app
                .UseHomePage()
                .UseCubesSwagger()
                .UseStaticContent(configuration, loggerFactory)
                .UseCubesMiddleware(loggerFactory);
        }

        public static IApplicationBuilder UseCubesMiddleware(this IApplicationBuilder app, ILoggerFactory loggerFactory)
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
                // TODO return Json with info
                context.HttpContext.Response.ContentType = "text/plain";
                await context.HttpContext.Response.WriteAsync(
                    "Status code page, status code: " +
                    context.HttpContext.Response.StatusCode);
            });

            return app;
        }

        public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder app,
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

                        await context
                            .Response
                            .WriteAsync(new ErrorDetails()
                            {
                                StatusCode = context.Response.StatusCode,
                                Message    = "An unhandled exception occurred while processing the request.",
                                Details    = details.ToString()
                            }
                            .ToString());
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

            public override string ToString() => JsonConvert.SerializeObject(this);
        }
    }
}
