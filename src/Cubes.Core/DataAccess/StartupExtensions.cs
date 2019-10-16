using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cubes.Core.DataAccess
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IDatabaseConnectionManager, DatabaseConnectionManager>();
            services.AddScoped<ISqlQueryManager, SqlQueryManager>();
            services.AddScoped<IQueryExecutor, QueryExecutor>();

            services.Configure<DataAccessSettings>(configuration.GetSection(nameof(DataAccessSettings)));

            return services;
        }
    }
}
