using System.Text;
using Cubes.Core.Base;
using Cubes.Core.Commands;
using Cubes.Core.Configuration;
using Cubes.Core.DataAccess;
using Cubes.Core.Email;
using Cubes.Core.Metrics;
using Cubes.Core.Scheduling;
using Cubes.Core.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Cubes.Core
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddCubesCore(this IServiceCollection services, IConfiguration configuration)
        {
            // Encodings
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var cubesConfig = configuration.GetCubesConfiguration();
            services.AddDataAccess(configuration)
                .AddEmailDispatcher(configuration)
                .AddScheduler(typeof(StartupExtensions).Assembly)
                .AddMetrics(configuration)
                .AddSingleton<IContextProvider, ContextProvider>()
                .AddTransient<ITypeResolver, TypeResolver>()
                .AddTransient<ILocalStorage>(_ => new LocalStorage(cubesConfig.StorageFolder))
                .AddTransient<IConfigurationWriter, ConfigurationWriter>()
                .Configure<CubesConfiguration>(configuration.GetSection(CubesConstants.Configuration_Section))
                .Configure<RequestLoggingOptions>(configuration.GetSection(nameof(RequestLoggingOptions)));

            // Global JSON convert settings
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
            };
            services.AddSingleton(serializerSettings);

            return services;
        }

        public static IConfigurationBuilder AddCubesConfiguration(
            this IConfigurationBuilder config,
            ICubesEnvironment cubesEnvironment)
        {
            config.AddCubesConfigurationProvider(cubesEnvironment);
            config.AddYamlFile(
                cubesEnvironment.GetFileOnPath(CubesFolderKind.Config, CubesConstants.Files_DataAccess),
                optional: true,
                reloadOnChange: true);
            config.AddYamlFile(
                cubesEnvironment.GetFileOnPath(CubesFolderKind.Config, CubesConstants.Files_StaticContent),
                optional: true,
                reloadOnChange: true);
            config.AddYamlFile(
                cubesEnvironment.GetFileOnPath(CubesFolderKind.Config, CubesConstants.Files_SmtpSettings),
                optional: true,
                reloadOnChange: true);

            // Installation specific application settings
            config.AddJsonFile(
                cubesEnvironment.GetFileOnPath(CubesFolderKind.Config, CubesConstants.Files_AppSettings),
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