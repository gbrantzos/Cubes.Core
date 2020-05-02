using Cubes.Core.DataAccess;
using Cubes.Core.Email;
using Cubes.Core.Base;
using Cubes.Core.Scheduling;
using Cubes.Core.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Cubes.Core.Configuration;

namespace Cubes.Core
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddCubesCore(this IServiceCollection services, IConfiguration configuration)
        {
            var cubesConfig = configuration.GetCubesConfiguration();

            services.AddDataAccess(configuration)
                .AddEmailDispatcher(configuration)
                .AddScheduler(typeof(StartupExtensions).Assembly)
                .AddTransient<ITypeResolver, TypeResolver>()
                .AddTransient<ILocalStorage>(s => new LocalStorage(cubesConfig.StorageFolder))
                .AddSingleton<IContextProvider, ContextProvider>()
                .AddTransient<IConfigurationWriter, ConfigurationWriter>()
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
            config.AddYamlFile(
                cubesEnvironment.GetFileOnPath(CubesFolderKind.Settings, CubesConstants.Files_StaticContent),
                optional: true,
                reloadOnChange: true);
            config.AddYamlFile(
                cubesEnvironment.GetFileOnPath(CubesFolderKind.Settings, CubesConstants.Files_SmtpSettings),
                optional: true,
                reloadOnChange: true);

            return config;
        }

        public static IServiceCollection AddApplicationsServices(this IServiceCollection services,
            IConfiguration configuration,
            ICubesEnvironment cubes)
        {
            services.AddSingleton(cubes);
            foreach (var application in cubes.GetApplicationInstances())
                application.ConfigureServices(services, configuration);

            return services;
        }

        public static IConfigurationBuilder AddApplicationsConfiguration(this IConfigurationBuilder configuration,
            ICubesEnvironment cubes)
        {
            foreach (var application in cubes.GetApplicationInstances())
                application.ConfigureAppConfiguration(configuration, cubes);

            return configuration;
        }
    }
}