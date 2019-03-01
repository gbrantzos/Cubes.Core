using Cubes.Core.Environment;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Web;
using System;
using System.IO;
using System.Reflection;

namespace Cubes.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var rootFolder = GetRootFolder();
                NLogHelpers.PrepareNLog(rootFolder);

                var logger = new NLogLoggerProvider().CreateLogger(typeof(CubesEnvironment).FullName);
                var cubesEnvironment = new CubesEnvironment(rootFolder, logger);
                cubesEnvironment.PrepareEnvironmentFolders();
                cubesEnvironment.LoadAppsAssemblies();

                CreateWebHostBuilder(args, cubesEnvironment, logger)
                    .Build()
                    .Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cubes Server stopped because of exception!");
                Console.WriteLine(ex.ToString());

                new NLogLoggerProvider()
                    .CreateLogger(typeof(CubesEnvironment).FullName)
                    .LogError(ex, "Cubes Server stopped because of exception!");
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
            return String.IsNullOrEmpty(rootFolder) ? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) : rootFolder;
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args, ICubesEnvironment cubesEnvironment, ILogger logger)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(cubesEnvironment.GetRootFolder())
                .AddJsonFile("host.json", optional: true);

            var configuration = builder.Build();
            var urls = configuration.GetValue<string>("urls", "http://localhost:3001");
            logger.LogInformation($"Cubes listening at {urls.Replace(';' ,',')}");

            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseContentRoot(cubesEnvironment.GetFolder(CubesFolderKind.StaticContent))
                .UseUrls(urls)
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ICubesEnvironment>(cubesEnvironment);
                }).
                ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                })
                .UseConfiguration(configuration)
                .UseNLog();
        }
    }
}
