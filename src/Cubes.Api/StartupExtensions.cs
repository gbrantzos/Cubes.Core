using System.IO;
using Cubes.Api.RequestContext;
using Cubes.Api.StaticContent;
using Cubes.Core.Environment;
using Cubes.Core.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace Cubes.Api
{
    public static class StartupExtensions
    {
        public static void AddCubesApi(this IServiceCollection services, ICubesEnvironment cubesEnvironment)
        {
            services.AddScoped<IContextProvider, ContextProvider>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddCubesSwaggerServices(cubesEnvironment);
        }

        public static IApplicationBuilder UseCubesApi(this IApplicationBuilder app,
            ISettingsProvider settingsProvider,
            ICubesEnvironment environment,
            ILoggerFactory loggerFactory)
        {
            return app
                .UseStaticContent(settingsProvider, environment, loggerFactory)
                .UseHomePage()
                .UseContextProvider()
                .UseCubesSwagger();
        }
    }
}
