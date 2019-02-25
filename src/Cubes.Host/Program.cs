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

namespace Cubes.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var cubesEnvironment = new CubesEnvironment();
            cubesEnvironment.PrepareEnvironmentFolders();
            cubesEnvironment.LoadAppsAssemblies();
            cubesEnvironment.EnsureDefaultLoggersForNLog();

            CreateWebHostBuilder(args, cubesEnvironment)
                .Build()
                .Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args, ICubesEnvironment cubesEnvironment) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ICubesEnvironment>(cubesEnvironment);
                });
    }
}
