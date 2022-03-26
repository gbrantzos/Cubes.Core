using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cubes.Core.DataAccess
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
            => services
                .AddScoped<IConnectionManager, ConnectionManager>()
                .AddScoped<IQueryManager, QueryManager>()
                .AddSingleton<IDefaultQueries, DefaultQueries>()
                .Configure<DataAccessOptions>(configuration.GetSection(nameof(DataAccessOptions)));
    }
}