using System.Collections.Generic;
using Autofac;
using Cubes.Core.Web.UIHelpers.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cubes.Core.Base
{
    /// <summary>
    /// Cubes Application
    /// </summary>
    public interface IApplication
    {
        /// <summary>
        /// Configure Application configuration, .Net core specific
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="cubes"></param>
        /// <returns></returns>
        IConfigurationBuilder ConfigureAppConfiguration(IConfigurationBuilder configuration, ICubesEnvironment cubes);

        /// <summary>
        /// Configure Services, .Net core specific
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        IServiceCollection ConfigureServices(IServiceCollection services, IConfiguration configuration);

        /// <summary>
        /// Register Services on Container, Autofac specific
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        ContainerBuilder RegisterServices(ContainerBuilder builder);

        /// <summary>
        /// File to be used for Swagger documentation
        /// </summary>
        IEnumerable<string> GetSwaggerXmlFiles();

        /// <summary>
        /// Information needed to enable UI design for application settings.
        /// </summary>
        /// <returns></returns>
        IEnumerable<ApplicationSettingsUIConfig> GetUISettings();
    }

    public class ApplicationSettingsUIConfig
    {
        public string DisplayName      { get; set; }
        public string SettingsTypeName { get; set; }
        public ComplexSchema UISchema  { get; set; }
        public string AssemblyName     { get; set; }
        public string AssemblyPath     { get; set; }
    }

}
