using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cubes.Core.Base.Samples
{
    public class SampleApplication : Application
    {
        public const string SettingFile = "Core.SampleApplication.yaml";

        public override IEnumerable<ApplicationSettingsUIConfig> GetUISettings()
        {
            return base
                .GetUISettings()
                .Append(new ApplicationSettingsUIConfig
                {
                    DisplayName      = "Sample Application",
                    SettingsTypeName = "Cubes.Core.Base.Samples.SampleApplicationSettings",
                    UISchema         = SampleApplicationSettingsSchema.GetSchema(),
                    AssemblyName     = this.GetType().Assembly.GetName().Name,
                    AssemblyPath     = this.GetType().Assembly.Location
                });
        }

        public override IServiceCollection ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            return base.ConfigureServices(services, configuration)
                .Configure<SampleApplicationSettings>(configuration.GetSection(nameof(SampleApplicationSettings)));
        }

        public override IConfigurationBuilder ConfigureAppConfiguration(IConfigurationBuilder configuration, ICubesEnvironment cubes)
        {
            return configuration.AddYamlFile(
                cubes.GetFileOnPath(CubesFolderKind.Settings, SampleApplication.SettingFile),
                optional: true,
                reloadOnChange: true);
        }
    }
}
