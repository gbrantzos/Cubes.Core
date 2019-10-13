using System;
using System.IO;
using Cubes.Core.Environment;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Web;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

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
                using ILoggerProvider loggerProvider = GetNLogProvider();
                var rootFolder = GetRootFolder();
                var cubesEnvironment = new CubesEnvironment(rootFolder,
                    loggerProvider.CreateLogger(typeof(CubesEnvironment).FullName));

                cubesEnvironment.PrepareEnvironment();
                cubesEnvironment.LoadAppsAssemblies();

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

        public static IHostBuilder CreateHostBuilder(string[] args, ICubesEnvironment cubesEnvironment)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(cubesEnvironment.GetRootFolder())
                .AddJsonFile("appsettings.json", optional: false)
                .AddCommandLine(args);

            var configuration = builder.Build();
            var urls = configuration.GetValue<string>("Host:URLs", "http://localhost:3001");

            return Microsoft.Extensions.Hosting.Host
                .CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((builder, config) =>
                {
                    // TODO Possibly method on Cubes Environment
                    var cubesFolders = new Dictionary<string, string>
                    {
                        { "Cubes.RootFolder"    , cubesEnvironment.GetRootFolder()  },
                        { "Cubes.AppsFolder"    , cubesEnvironment.GetAppsFolder()  },
                        { "Cubes.StorageFolder" , cubesEnvironment.GetStorageFolder()  },
                    };
                    config.AddInMemoryCollection(cubesFolders);
                })
                .ConfigureServices((builder, services) => services.AddSingleton(cubesEnvironment))
                .ConfigureLogging(logging => logging.ClearProviders())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls(urls);
                })
                .UseContentRoot(cubesEnvironment.GetFolder(CubesFolderKind.StaticContent))
                .UseNLog();
        }

        private static ILoggerProvider GetNLogProvider()
        {
            var programPath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            var configFile  = Path.Combine(programPath, "NLog.config");
            if (!File.Exists(configFile))
            {
                var sampleFile = Path.Combine(programPath, "NLog.Sample.config");
                File.Copy(sampleFile, configFile);
            }
            NLogBuilder.ConfigureNLog(configFile);

            return new NLogLoggerProvider();
        }
    }
}
