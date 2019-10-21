using System;
using System.Linq;
using Cubes.Core.Environment;
using Cubes.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cubes.Host
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration) => this.configuration = configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Cubes Configuration
            var cubesAssemblies = configuration
                .GetCubesConfiguration()
                .AssembliesWithControllers ?? Array.Empty<string>();

            var assemblies = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .Where(asm => cubesAssemblies.Contains(asm.GetName().Name))
                .ToList();

            // Setup WebAPI
            var mvcBuilder = services.AddControllers()
                .AddNewtonsoftJson();

            // Add applications assemblies
            foreach (var asm in assemblies)
                mvcBuilder.AddApplicationPart(asm);

            // Setup Cubes
            services.AddCubesWeb(configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            var useSSL           = configuration.GetValue<bool>(CubesConstants.Config_HostUseSSL, false);
            var loggerFactory    = app.ApplicationServices.GetService<ILoggerFactory>();
            var cubesEnvironment = app.ApplicationServices.GetService<ICubesEnvironment>();

            if (useSSL)
            {
                // The default HSTS value is 30 days.
                // You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            // Should be called as soon as possible.
            app.UseCubesApi(configuration, loggerFactory);

            // Routing
            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
       }
    }
}
