using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cubes.Core.DataAccess
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IConnectionManager, ConnectionManager>();
            services.AddScoped<IQueryManager, QueryManager>();
            services.AddSingleton<IDefaultQueries, DefaultQueries>();
            services.Configure<DataAccessOptions>(configuration.GetSection(nameof(DataAccessOptions)));

            return services;
        }
    }
}
