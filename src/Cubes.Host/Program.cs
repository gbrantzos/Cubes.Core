using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cubes.Core.Environment;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace Cubes.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logFactory = NLogBuilder.ConfigureNLog("NLog.Sample.config");
            var logger = logFactory.GetCurrentClassLogger();
            try
            {
                var cubesEnvironment = new CubesEnvironment();
                cubesEnvironment.PrepareEnvironmentFolders();
                cubesEnvironment.LoadAppsAssemblies();
                cubesEnvironment.EnsureDefaultLoggersForNLog();

                CreateWebHostBuilder(args, cubesEnvironment)
                    .Build()
                    .Run();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args, ICubesEnvironment cubesEnvironment) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ICubesEnvironment>(cubesEnvironment);
                }).
                ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                })
                .UseNLog();
    }
}
