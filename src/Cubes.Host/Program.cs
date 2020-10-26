using System;
using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Cubes.Core;
using Cubes.Core.Base;
using Cubes.Core.Utilities;
using Cubes.Core.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
using NLog.Config;

#if (DEBUG && WINDOWS)
using System.Diagnostics;
using System.Runtime.InteropServices;
#endif

namespace Cubes.Host
{
    public static class Program
    {
#if (DEBUG && WINDOWS)
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int cmdShow);
        private const int HIDE = 0;
        private const int MAXIMIZE = 3;
        private const int MINIMIZE = 6;
        private const int RESTORE = 9;
#endif
        public static void Main(string[] args)
        {
#if (DEBUG && WINDOWS)
            Process p = Process.GetCurrentProcess();
            ShowWindow(p.MainWindowHandle, MAXIMIZE);
#endif

            try
            {
                var rootFolder   = CubesEnvironmentHelpers.GetRootFolder(args);
                var adminPath    = CubesEnvironmentHelpers.GetAdminPath(args);
                var applications = CubesEnvironmentHelpers.GetApplications(rootFolder, args);

                using var loggerProvider = GetNLogProvider(rootFolder);
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
                LogManager.Shutdown();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args, ICubesEnvironment cubes)
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
            var binariesPath = Path.GetDirectoryName(typeof(Program).Assembly.Location) ?? basedir;
            var configFile = Path.Combine(binariesPath, CubesConstants.NLog_ConfigFile);
            if (!File.Exists(configFile))
            {
                var sampleFile = Path.Combine(binariesPath, CubesConstants.NLog_SampleFile);
                File.Copy(sampleFile, configFile);
            }
            var installationConfig = Path.Combine(basedir,
                nameof(CubesFolderKind.Config),
                CubesConstants.NLog_ConfigFile);
            NLogBuilder.ConfigureNLog(File.Exists(installationConfig) ? installationConfig : configFile);
            LogManager.Configuration.Variables["cubesRoot"] = basedir;
            ConfigurationItemFactory.Default.JsonConverter = new NLogJsonSerializer();

            return new NLogLoggerProvider();
        }
    }
}
