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
            services.AddScoped<IDefaultQueries, DefaultQueries>();
            services.AddTransient<IQueryProvider, SampleQueryProvider>();
            services.Configure<DataAccessSettings>(configuration.GetSection(nameof(DataAccessSettings)));

            return services;
        }
    }
}
