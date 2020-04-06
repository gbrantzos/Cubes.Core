using Cubes.Core.Web.UIHelpers.Lookups;
using Cubes.Core.Web.UIHelpers.Schema;
using Microsoft.Extensions.DependencyInjection;

namespace Cubes.Core.Web.UIHelpers
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
