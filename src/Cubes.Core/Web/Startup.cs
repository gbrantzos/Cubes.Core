using System;
using System.Linq;
using Cubes.Core.Base;
using Cubes.Core.Web.ResponseWrapping;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Cubes.Core.Web
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration) => this.configuration = configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var assemblies = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .Where(asm => asm.GetTypes().Any(t => t.IsSubclassOf(typeof(ControllerBase))))
                .ToList();

            // Setup WebAPI
            var mvcBuilder = services
                .AddControllers()
                .AddControllersAsServices()
                .AddNewtonsoftJson();

            // Add applications assemblies
            foreach (var asm in assemblies)
            {
                mvcBuilder
                    .AddApplicationPart(asm)
                    .AddControllersAsServices();
            }

            // Setup Cubes
            services.AddCubesWeb(configuration, mvcBuilder);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var useSsl             = configuration.GetValue<bool>(CubesConstants.Config_HostUseSSL, false);
            var loggerFactory      = app.ApplicationServices.GetService<ILoggerFactory>();
            var responseBuilder    = app.ApplicationServices.GetService<IApiResponseBuilder>();
            var serializerSettings = app.ApplicationServices.GetService<JsonSerializerSettings>();

            if (useSsl)
            {
                // The default HSTS value is 30 days.
                // You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            // Server files from WebRoot folder
            app.UseStaticFiles();

            // Should be called as soon as possible.
            app.UseCubesApi(configuration, env, responseBuilder, loggerFactory, serializerSettings);

            // Routing
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapControllers());
       }
    }
}
