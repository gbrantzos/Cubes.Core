using System.Linq;
using Cubes.Core.Commands;
using Cubes.Core.DataAccess;
using Cubes.Core.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Cubes.Core.Environment
{
    public static class StartupExtensions
    {
        public static void AddCubes(this IServiceCollection services)
        {
            // Add standard services
            services.AddSettings();
            services.AddDataAccess();
            services.AddCommands();
        }

        public static void AddDataAccess(this IServiceCollection services)
        {
            services.AddScoped<IDatabaseConnectionManager, DatabaseConnectionManager>();
            services.AddScoped<ISqlQueryManager, SqlQueryManager>();
            services.AddScoped<IQueryExecutor, QueryExecutor>();
        }

        public static void AddCommands(this IServiceCollection services)
        {
            services.AddScoped<ServiceFactory>(p => p.GetService);
            services.AddScoped<ICommandBus, CommandBus>();

            // Add command handlers
            services.Scan(s => s
                .FromApplicationDependencies()
                .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            // Add command middleware
            services.Scan(s => s
                .FromApplicationDependencies()
                .AddClasses(c => c.AssignableTo(typeof(ICommandBusMiddleware<,>)))
                .AsImplementedInterfaces());

            /*
            var types = System.AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(t => t.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICommandBusMiddleware<,>)))
                .ToList();
            types.ForEach(t => services.AddScoped(typeof(ICommandBusMiddleware<,>), t));
             */
        }

        public static void AddSettings(this IServiceCollection services)
            => services.AddSingleton<ISettingsProvider>(s => new JsonFilesSettingsProvider(s.GetService<ICubesEnvironment>().GetSettingsFolder()));
    }
}