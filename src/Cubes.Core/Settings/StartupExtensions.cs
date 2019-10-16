using System;
using Cubes.Core.Environment;
using Microsoft.Extensions.DependencyInjection;

namespace Cubes.Core.Settings
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddSettings(this IServiceCollection services, string settingsFormat)
        {
            if (!(settingsFormat.Equals("json", StringComparison.CurrentCultureIgnoreCase)) &&
                !(settingsFormat.Equals("yaml", StringComparison.CurrentCultureIgnoreCase)))
                throw new ArgumentException($"Unsupported settings format: '{settingsFormat}");

            if (settingsFormat.Equals("json", StringComparison.CurrentCultureIgnoreCase))
                services.AddSingleton<ISettingsProvider>(s => new JsonFilesSettingsProvider(s.GetService<ICubesEnvironment>().GetSettingsFolder()));
            if (settingsFormat.Equals("yaml", StringComparison.CurrentCultureIgnoreCase))
                services.AddSingleton<ISettingsProvider>(s => new YamlFilesSettingsProvider(s.GetService<ICubesEnvironment>().GetSettingsFolder()));

            return services;
        }
    }
}