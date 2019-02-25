using Cubes.Core.Environment;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
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

                var cubesEnvironment = new CubesEnvironment(rootFolder, new NLogLoggerProvider().CreateLogger(typeof(CubesEnvironment).FullName));
                cubesEnvironment.PrepareEnvironmentFolders();
                cubesEnvironment.LoadAppsAssemblies();

                CreateWebHostBuilder(args, cubesEnvironment)
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

        public static IWebHostBuilder CreateWebHostBuilder(string[] args, ICubesEnvironment cubesEnvironment) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseContentRoot(cubesEnvironment.GetFolder(CubesFolderKind.StaticContent))
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ICubesEnvironment>(cubesEnvironment);
                }).
                ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                })
                .UseNLog();

    }
}
