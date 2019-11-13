using Cubes.Web.UIHelpers.Lookups;
using Cubes.Web.UIHelpers.Schema;
using Microsoft.Extensions.DependencyInjection;

namespace Cubes.Web.UIHelpers
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddUIServices(this IServiceCollection services)
        {
            services.AddTransient<ILookupProvider, RequestTypeLokkupProvider>();
            services.AddTransient<ILookupProvider, JobTypeLookupProvider>();

            services.AddSchemaServices();

            return services;
        }
    }
}
