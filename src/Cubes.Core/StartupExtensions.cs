using Cubes.Core.Commands;
using Cubes.Core.DataAccess;
using Cubes.Core.Email;
using Cubes.Core.Jobs;
using Cubes.Core.Settings;
using Cubes.Core.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cubes.Core
{
    public static class StartupExtensions
    {
        public static void AddCubesCore(this IServiceCollection services, IConfiguration configuration)
        {
            var settingsFormat = configuration.GetValue<string>("settingsFormat", "yaml");

            // Add standard services
            services.AddSettings(settingsFormat);
            services.AddDataAccess();
            services.AddCommands();
            services.AddEmailDispatcher();
            services.AddJobScheduler();
            services.AddTransient<ITypeResolver, TypeResolver>();
            services.AddTransient<ISerializer, JsonSerializer>();
        }
    }
}