using Cubes.Core;
using Cubes.Core.Environment;
using Cubes.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
            // Setup WebAPI
            services.AddControllers();

            // Setup Cubes
            services.AddCubesCore(configuration);
            services.AddCubesWeb(configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var useSSL = configuration.GetValue<bool>("Host:UseSSL", false);
            if (useSSL)
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            var cubesEnvironment  = app.ApplicationServices.GetService<ICubesEnvironment>();
            var loggerFactory     = app.ApplicationServices.GetService<ILoggerFactory>();
            var enableCompression = configuration.GetValue<bool>("Host:EnableCompression", true);

            // Should be called as soon as possible.
            app.UseCubesApi(null, //TODO Replace with IOptions  - settingsProvider,
                cubesEnvironment,
                loggerFactory,
              enableCompression);

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Finally start Cubes
            cubesEnvironment.Start(serviceProvider: app.ApplicationServices);
        }
    }
}
