using Cubes.Core.Web.UIHelpers.Lookups;
using Cubes.Core.Web.UIHelpers.Lookups.Providers;
using Cubes.Core.Web.UIHelpers.Schema;
using Cubes.Core.Web.UIHelpers.Schema.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace Cubes.Core.Web.UIHelpers
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddUIServices(this IServiceCollection services)
        {
            services
                .AddSingleton<ISchemaManager, SchemaManager>()
                .AddTransient<ISchemaProvider, SmtpSettingsProfilesSchemaProvider>()
                .AddTransient<ISchemaProvider, ConnectionSchemaProvider>()
                .AddTransient<ISchemaProvider, QuerySchemaProvider>()
                .AddTransient<ISchemaProvider, StaticContentSchemaProvider>()

                .AddScoped<ILookupManager, LookupManager>()
                .AddTransient<ILookupProvider, RequestTypeLookupProvider>()
                .AddTransient<ILookupProvider, JobTypeLookupProvider>()
                .AddTransient<ILookupProvider, DatabaseProvidersLookupProvider>()
                .AddTransient<ILookupProvider, DataConnectionLookupProvider>()
                .AddTransient<ILookupProvider, DataQueryLookupProvider>()
                .AddTransient<ILookupProvider, SmtpSettingsLookupProvider>();

            return services;
        }
    }
}
