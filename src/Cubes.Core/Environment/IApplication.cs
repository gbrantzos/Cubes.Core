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

        // Swagger documentation file
        string SwaggerXmlFile { get; }

        // Configure http pipeline
    }
}
