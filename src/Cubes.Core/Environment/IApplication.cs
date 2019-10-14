using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cubes.Core.Environment
{
    public interface IApplication
    {
        // Configure Application configuration
        IConfiguration ConfigureAppConfiguration(IConfiguration configuration);

        // Configure Services
        IServiceCollection ConfigureServices(IServiceCollection services);

        // Configure http pipeline
    }
}
