using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using Cubes.Api.Filters;
using Cubes.Api.RequestContext;
using Cubes.Api.StaticContent;
using Cubes.Core.Environment;
using Cubes.Core.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Cubes.Api
{
    public static class StartupExtensions
    {
        public static void AddCubesApi(this IServiceCollection services, ICubesEnvironment cubesEnvironment, bool enableCompression)
        {
            services.AddScoped<IContextProvider, ContextProvider>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            if (enableCompression)
                services.AddResponseCompression();
            services.AddCubesSwaggerServices(cubesEnvironment);
        }

        public static IApplicationBuilder UseCubesApi(this IApplicationBuilder app,
            ISettingsProvider settingsProvider,
            ICubesEnvironment environment,
            ILoggerFactory loggerFactory,
            bool enableCompression)
        {
            if (enableCompression)
                app.UseResponseCompression();
            return app
                .UseCustomExceptionHandler(loggerFactory)
                .UseStaticContent(settingsProvider, environment, loggerFactory)
                .UseHomePage()
                .UseContextProvider()
                .UseCubesSwagger();
        }


        public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("Cubes.Api.CustomExceptionHandler");
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
                        logger.LogError(ex, $"Unhandled exception: {ex.Message}");

                        var frame   = new StackTrace(ex, true).GetFrame(0);
                        var details = new StringBuilder();
                        var methodInfo = $"{frame.GetMethod().DeclaringType.FullName}.{frame.GetMethod().Name}()";
                        var fileInfo = $"{Path.GetFileName(frame.GetFileName())}, line {frame.GetFileLineNumber()}";
                        details.AppendLine($"{ex.GetType().Name}: {ex.Message}");
                        details.AppendLine($"{methodInfo} in {fileInfo}");

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

        public static MvcOptions AddCubesApiMvcOptions(this MvcOptions options)
        {
            options.Filters.Add(typeof(ValidateModelFilterAttribute));
            return options;
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
