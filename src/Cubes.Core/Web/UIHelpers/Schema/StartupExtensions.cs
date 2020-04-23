using Cubes.Core.Web.UIHelpers.Schema.CoreProviders;
using Microsoft.Extensions.DependencyInjection;

namespace Cubes.Core.Web.UIHelpers.Schema
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddSchemaServices(this IServiceCollection services)
            => services
                .AddSingleton<ISchemaManager, SchemaManager>()
                .AddTransient<ISchemaProvider, SmtpSettingsProfilesSchemaProvider>()
                .AddTransient<ISchemaProvider, ConnectionSchemaProvider>()
                .AddTransient<ISchemaProvider, QuerySchemaProvider>()
                .AddTransient<ISchemaProvider, StaticContentSchemaProvider>();
    }
}
