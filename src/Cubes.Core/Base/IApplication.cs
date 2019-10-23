using Autofac;
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
        /// <returns></returns>
        IConfigurationBuilder ConfigureAppConfiguration(IConfigurationBuilder configuration);

        /// <summary>
        /// Configure Services, .Net core specific
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        IServiceCollection ConfigureServices(IServiceCollection services);

        /// <summary>
        /// Register Services on Container, Autofac specific
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        ContainerBuilder RegisterServices(ContainerBuilder builder);

        /// <summary>
        /// File to be used for Swagger documentation
        /// </summary>
        string SwaggerXmlFile { get; }

        /// <summary>
        /// Comma separated list of assemblies that contain Controllers
        /// </summary>
        string[] AssembliesWithControllers { get; }
    }
}
