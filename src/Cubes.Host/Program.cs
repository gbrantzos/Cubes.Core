using System;
using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Cubes.Core;
using Cubes.Core.Base;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using NLog.Extensions.Logging;

#if DEBUG
using System.Diagnostics;
using System.Runtime.InteropServices;
#endif


namespace Cubes.Host
{
    public class Program
    {
#if DEBUG
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int cmdShow);
        private const int HIDE = 0;
        private const int MAXIMIZE = 3;
        private const int MINIMIZE = 6;
        private const int RESTORE = 9;
#endif

        public static void Main(string[] args)
        {
#if DEBUG
            Process p = Process.GetCurrentProcess();
            ShowWindow(p.MainWindowHandle, MAXIMIZE);
#endif
            try
            {
                var rootFolder = GetRootFolder();
                using ILoggerProvider loggerProvider = GetNLogProvider(rootFolder);
                var cubesEnvironment = new CubesEnvironment(rootFolder,
                    loggerProvider.CreateLogger(typeof(CubesEnvironment).FullName));
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
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads
                // before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }

        private static string GetRootFolder()
        {
            // Check for environment variable, else get executing assembly path
            // We should support starting from script and Cubes folder are outside
            // binaries folder.
            var rootFolder = Environment.GetEnvironmentVariable("CUBES_ROOTFOLDER");
            return String.IsNullOrEmpty(rootFolder) ?
                Path.GetDirectoryName(typeof(Program).Assembly.Location) : rootFolder;
        }

        public static IHostBuilder CreateHostBuilder(string[] args, ICubesEnvironment cubes)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(cubes.GetBinariesFolder())
                .AddJsonFile(CubesConstants.Config_AppSettings, optional: false)
                .AddCommandLine(args)
                .Build();
            var urls = configuration.GetValue<string>(CubesConstants.Config_HostURLs, "http://localhost:3001");

            return Microsoft.Extensions.Hosting.Host
                .CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .UseWindowsService()
                .UseContentRoot(cubes.GetRootFolder())
                .ConfigureAppConfiguration((builder, config) =>
                {
                    config.AddCubesConfiguration(cubes);
                    config.AddApplicationsConfiguration(cubes);
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddNLog();
                })
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                    builder
                        .RegisterCubeServices()
                        .RegisterApplicationServices(cubes);
                })
                .ConfigureServices((builder, services) =>
                {
                    services.AddSingleton(cubes);
                    services.AddCubesCore(builder.Configuration);

                    services.AddApplicationsServices(cubes);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls(urls);
                    webBuilder.UseWebRoot(cubes.GetFolder(CubesFolderKind.Content));
                });
        }

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
            NLog.LogManager.Configuration.Variables["basedir"] = basedir;

            return new NLogLoggerProvider();
        }
    }
}
