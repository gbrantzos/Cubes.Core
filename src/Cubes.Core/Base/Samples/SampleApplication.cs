using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cubes.Core.Base.Samples
{
    public class SampleApplication : Application
    {
        public const string OptionsFile = "Core.SampleApplication.yaml";

        public override IEnumerable<ApplicationOptionsUIConfig> GetUISettings()
        {
            return base
                .GetUISettings()
                .Append(new ApplicationOptionsUIConfig
                {
                    DisplayName     = "Sample Application",
                    OptionsTypeName = "Cubes.Core.Base.Samples.SampleApplicationOptions",
                    UISchema        = SampleApplicationOptionsSchema.GetSchema(),
                    AssemblyName    = this.GetType().Assembly.GetName().Name,
                    AssemblyPath    = this.GetType().Assembly.Location
                });
        }

        public override IServiceCollection ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            return base.ConfigureServices(services, configuration)
                .Configure<SampleApplicationOptions>(configuration.GetSection(nameof(SampleApplicationOptions)));
        }

        public override IConfigurationBuilder ConfigureAppConfiguration(IConfigurationBuilder configuration, ICubesEnvironment cubes)
        {
            return configuration.AddYamlFile(
                cubes.GetFileOnPath(CubesFolderKind.Settings, SampleApplication.OptionsFile),
                optional: true,
                reloadOnChange: true);
        }
    }
}
