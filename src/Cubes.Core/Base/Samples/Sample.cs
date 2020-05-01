using System;
using System.Collections.Generic;
using System.Linq;
using Cubes.Core.Web.UIHelpers;
using Cubes.Core.Web.UIHelpers.Lookups;
using Cubes.Core.Web.UIHelpers.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cubes.Core.Base.Samples
{
    public class SampleApplication : Application
    {
        public override IEnumerable<ApplicationSettingsUIConfig> GetUISettings()
        {
            return base
                .GetUISettings()
                .Append(new ApplicationSettingsUIConfig
                {
                    DisplayName      = "Sample Application",
                    SettingsTypeName = "Cubes.Core.Base.Samples.SampleApplicationSettings",
                    UISchema         = this.GetSchema(),
                    AssemblyName     = this.GetType().Assembly.GetName().Name,
                    AssemblyPath     = this.GetType().Assembly.Location
                });
        }

        public override IServiceCollection ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            return base.ConfigureServices(services, configuration)
                .Configure<SampleApplicationSettings>(configuration.GetSection(nameof(SampleApplicationSettings)));
        }

        private ComplexSchema GetSchema()
        {
            var cs = new ComplexSchema
            {
                Name = "SampleApplication"
            };
            cs.Sections.Add(new ComplexSchemaSection
            {
                RootProperty   = "Basic",
                Schema         = Schema.Create("Basic")
                    .WithSelectDynamic("SEnConnection", "SEn Connection", LookupProviders.DataConnections)
                    .WithSelectDynamic("OdwConnection", "ODW Connection", LookupProviders.DataConnections)
            });
            cs.Sections.Add(new ComplexSchemaSection
            {
                RootProperty   = "WmsUsers",
                Schema         = Schema.Create("Users", "WMS Users")
                    .WithText("UserName", Validator.Required())
                    .WithText("DisplayName", Validator.Required())
                    .WithPassword("Password", Validator.Required()),
                IsList = true,
                ListItem = "displayName"
            });

            return cs;
        }
    }

    [ViewModelConverter(typeof(SampleApplicationSettingsViewModel))]
    public class SampleApplicationSettings
    {
        public string SEnConnection { get; set; } = "Pharmex.SEn";
        public string OdwConnection { get; set; } = "Pharmex.ODW";
    }

    public class SampleApplicationSettingsViewModel : ViewModelConverter
    {
        public override object FromViewModel(object viewModel)
        {
            var toReturn = new SampleApplicationSettings();
            dynamic temp = viewModel;

            toReturn.SEnConnection = temp.Basic.SEnConnection;
            toReturn.OdwConnection = temp.Basic.OdwConnection;

            return toReturn;
        }

        public override object ToViewModel(object configurationInstance)
        {
            var config = configurationInstance as SampleApplicationSettings;
            if (config is null)
                throw new ArgumentException($"Could not cast to {nameof(SampleApplicationSettings)}");

            return new
            {
                Basic = new
                {
                    config.SEnConnection,
                    config.OdwConnection
                }
            };
        }
    }
}
