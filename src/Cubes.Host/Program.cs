using System;
using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Cubes.Core;
using Cubes.Core.Base;
using Cubes.Core.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using NLog.Extensions.Logging;
using Cubes.Core.Web;

namespace Cubes.Host
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var rootFolder   = CubesEnvironmentHelpers.GetRootFolder(args);
                var adminPath    = CubesEnvironmentHelpers.GetAdminPath(args);
                var applications = CubesEnvironmentHelpers.GetApplications(rootFolder, args);
                using ILoggerProvider loggerProvider = GetNLogProvider(rootFolder);

                var cubesEnvironment = new CubesEnvironment(
                    rootFolder   : rootFolder,
                    adminPath    : adminPath,
                    applications : applications,
                    logger       : loggerProvider.CreateLogger(typeof(CubesEnvironment).FullName));
                cubesEnvironment.PrepareHost();

                CreateHostBuilder(args, cubesEnvironment)
                    .Build()
                    .Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cubes Host stopped because of exception!");
                Console.WriteLine(ex.ToString());

                new NLogLoggerProvider()
                    .CreateLogger(typeof(CubesEnvironment).FullName)
                    .LogError(ex, "Cubes Host stopped because of exception!");
#if DEBUG
                Console.WriteLine("Press any key to continue ...");
                Console.ReadKey(true);
#endif
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads
                // before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args, ICubesEnvironment cubes)
            => Microsoft.Extensions.Hosting.Host
                .CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .UseWindowsService()
                .UseContentRoot(cubes.GetRootFolder())
                .ConfigureAppConfiguration((_, configBuilder) =>
                {
                    configBuilder
                        .SetBasePath(cubes.GetBinariesFolder())
                        .AddCubesConfiguration(cubes)
                        .AddApplicationsConfiguration(cubes);
                })
                .ConfigureLogging(logging =>
                {
                    logging
                        .ClearProviders()
                        .AddNLog();
                })
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                    builder
                        .RegisterCubeServices()
                        .RegisterApplicationServices(cubes);
                })
                .ConfigureServices((builder, services) =>
                {
                    services
                        .AddSingleton(cubes)
                        .AddCubesCore(builder.Configuration)
                        .AddApplicationsServices(builder.Configuration, cubes);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseStartup<Startup>()
                        .UseWebRoot(cubes.GetFolder(CubesFolderKind.WebRoot));
                });

        // This is the only Host specific method, and can be changed with no Core changes!
        private static ILoggerProvider GetNLogProvider(string basedir)
        {
            var programPath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            var configFile  = Path.Combine(programPath, CubesConstants.NLog_ConfigFile);
            if (!File.Exists(configFile))
            {
                var sampleFile = Path.Combine(programPath, CubesConstants.NLog_SampleFile);
                File.Copy(sampleFile, configFile);
            }
            NLogBuilder.ConfigureNLog(configFile);
            NLog.LogManager.Configuration.Variables["cubesRoot"] = basedir;

            return new NLogLoggerProvider();
        }
    }
}
