using System.Collections.Generic;
using Cubes.Core.Commands;
using Cubes.Core.DataAccess;
using Cubes.Core.Email;
using Cubes.Core.Environment;
using Cubes.Core.Jobs;
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
            var settingsFormat = configuration.GetValue<string>("settingsFormat", "yaml");

            // Add standard services
            services.AddSettings(settingsFormat); // TODO To be removed!
            services.AddDataAccess(configuration);
            services.AddCommands();
            services.AddEmailDispatcher();
            services.AddJobScheduler();
            services.AddTransient<ITypeResolver, TypeResolver>();
            services.AddTransient<ISerializer, JsonSerializer>();

            return services;
        }

        public static IConfigurationBuilder AddCubesConfiguration(
            this IConfigurationBuilder config,
            ICubesEnvironment cubesEnvironment)
        {
            // TODO Possibly method on Cubes Environment
            var cubesFolders = new Dictionary<string, string>
                {
                    { "Cubes:RootFolder"    , cubesEnvironment.GetRootFolder()  },
                    { "Cubes:AppsFolder"    , cubesEnvironment.GetAppsFolder()  },
                    { "Cubes:StorageFolder" , cubesEnvironment.GetStorageFolder()  },
                };
            config.AddInMemoryCollection(cubesFolders);

            config.AddYamlFile(
                cubesEnvironment.GetFileOnPath(CubesFolderKind.Settings, "Core.DataSettings.yaml"),
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