using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cubes.Core.Environment
{
    public interface IApplication
    {
        // Configure Application configuration
        IConfigurationBuilder ConfigureAppConfiguration(IConfigurationBuilder configuration);

        // Configure Services
        IServiceCollection ConfigureServices(IServiceCollection services);

        // Register Services on Container (Autofac)
        ContainerBuilder RegisterServices(ContainerBuilder builder);

        // Swagger documentation file
        string SwaggerXmlFile { get; }

        string[] AssembliesWithControllers { get; }
        // Configure http pipeline
    }
}
