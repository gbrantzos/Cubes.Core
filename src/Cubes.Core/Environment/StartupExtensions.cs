using System;
using Cubes.Core.Commands;
using Cubes.Core.DataAccess;
using Cubes.Core.Email;
using Cubes.Core.Settings;
using Cubes.Core.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cubes.Core.Environment
{
    public static class StartupExtensions
    {
        public static void AddCubesCoreServices(this IServiceCollection services, IConfiguration configuration)
        {
            var settingsFormat = configuration.GetValue<string>("settingsFormat", "yaml");

            // Add standard services
            services.AddSettings(settingsFormat);
            services.AddDataAccess();
            services.AddCommands();
            services.AddEmailDispatcher();

            services.AddScoped<ITypeResolver, TypeResolver>();
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

        public static void AddSettings(this IServiceCollection services, string settingsFormat)
        {
            if (!(settingsFormat.Equals("json", StringComparison.CurrentCultureIgnoreCase)) &&
                !(settingsFormat.Equals("yaml", StringComparison.CurrentCultureIgnoreCase)))
                throw new ArgumentException($"Unsupported settings format: '{settingsFormat}");

            if (settingsFormat.Equals("json", StringComparison.CurrentCultureIgnoreCase))
                services.AddSingleton<ISettingsProvider>(s => new JsonFilesSettingsProvider(s.GetService<ICubesEnvironment>().GetSettingsFolder()));
            if (settingsFormat.Equals("yaml", StringComparison.CurrentCultureIgnoreCase))
                services.AddSingleton<ISettingsProvider>(s => new YamlFilesSettingsProvider(s.GetService<ICubesEnvironment>().GetSettingsFolder()));
        }

        public static void AddEmailDispatcher(this IServiceCollection services)
            => services.AddScoped<IEmailDispatcher>(c => new EmailDispatcher(new SmtpClientWrapper()));
    }
}