using Cubes.Core.Commands.Middleware.ExecutionHistory;
using Microsoft.Extensions.DependencyInjection;

namespace Cubes.Core.Commands
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddCommands(this IServiceCollection services)
        {
            services.AddScoped<ServiceFactory>(p => p.GetService);
            services.AddScoped<ICommandBus, CommandBus>();
            services.AddSingleton<ICommandExecutionHistory, CommandExecutionHistory>();

            // Add command handlers
            services.Scan(s => s
                .FromApplicationDependencies()
                .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            // Add command middleware
            /*
            services.Scan(s => s
                .FromApplicationDependencies()
                .AddClasses(c => c.AssignableTo(typeof(ICommandBusMiddleware<,>)))
                .AsImplementedInterfaces());
            */

            services.AddTransient(typeof(ICommandBusMiddleware<,>), typeof(CommandExecutionHistoryMiddleware<,>));

            /*
            var types = System.AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(t => t.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICommandBusMiddleware<,>)))
                .ToList();
            types.ForEach(t => services.AddScoped(typeof(ICommandBusMiddleware<,>), t));
             */

            return services;
        }
    }
}
