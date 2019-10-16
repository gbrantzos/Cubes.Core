using Cubes.Core.Commands;
using Cubes.Core.DataAccess;
using Cubes.Core.Email;
using Cubes.Core.Environment;
using Cubes.Core.Scheduling;
using Cubes.Core.Settings;
using Cubes.Core.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cubes.Core
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddCubesCore(this IServiceCollection services, IConfiguration configuration)
        {
            //
            services.AddDataAccess(configuration)
                .AddSettings("yaml") // TODO To be removed!
                .AddCommands()
                .AddEmailDispatcher()
                .AddScheduler(typeof(StartupExtensions).Assembly)
                .AddTransient<ITypeResolver, TypeResolver>()
                .AddTransient<ISerializer, JsonSerializer>()
                .Configure<CubesConfiguration>(configuration.GetSection(CubesConstants.Configuration_Section));

            return services;
        }

        public static IConfigurationBuilder AddCubesConfiguration(
            this IConfigurationBuilder config,
            ICubesEnvironment cubesEnvironment)
        {
            config.AddCubesConfigurationProvider(cubesEnvironment);
            config.AddYamlFile(
                cubesEnvironment.GetFileOnPath(CubesFolderKind.Settings, CubesConstants.Files_DataAccess),
                optional: true,
                reloadOnChange: true);

            return config;
        }

        public static IServiceCollection AddApplicationsServices(this IServiceCollection services,
            ICubesEnvironment cubes)
        {
            services.AddSingleton(cubes);
            foreach (var application in cubes.GetActivatedApplications())
                application.ConfigureServices(services);

            return services;
        }

        public static IConfigurationBuilder AddApplicationsConfiguration(this IConfigurationBuilder configuration,
            ICubesEnvironment cubes)
        {
            foreach (var application in cubes.GetActivatedApplications())
                application.ConfigureAppConfiguration(configuration);

            return configuration;
        }
    }
}