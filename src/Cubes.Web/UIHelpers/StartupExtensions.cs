using Cubes.Web.UIHelpers.Lookups;
using Cubes.Web.UIHelpers.Schema;
using Microsoft.Extensions.DependencyInjection;

namespace Cubes.Web.UIHelpers
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddUIServices(this IServiceCollection services)
        {
            services.AddTransient<ILookupProvider, RequestTypeLookupProvider>();
            services.AddTransient<ILookupProvider, JobTypeLookupProvider>();
            services.AddTransient<ILookupProvider, DatabaseProvidersLookupProvider>();

            services.AddSchemaServices();

            return services;
        }
    }
}
