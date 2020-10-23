using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cubes.Core.Metrics
{
    public static class ServiceProviderExtensions
    {
        public static IServiceCollection AddMetrics(this IServiceCollection services, IConfiguration configuration)
        {
            CubesCoreMetrics.AddFromConfiguration(configuration);

            return services.AddSingleton<IMetrics>(new PrometheusMetrics());
        }
    }
}
