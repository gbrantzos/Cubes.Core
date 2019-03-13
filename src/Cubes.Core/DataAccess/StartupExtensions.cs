using Microsoft.Extensions.DependencyInjection;

namespace Cubes.Core.DataAccess
{
    public static class StartupExtensions
    {
        public static void AddDataAccess(this IServiceCollection services)
        {
            services.AddScoped<IDatabaseConnectionManager, DatabaseConnectionManager>();
            services.AddScoped<ISqlQueryManager, SqlQueryManager>();
            services.AddScoped<IQueryExecutor, QueryExecutor>();
        }
    }
}
