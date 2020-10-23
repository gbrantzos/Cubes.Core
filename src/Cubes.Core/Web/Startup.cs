using System;
using System.IO;
using System.Linq;
using System.Text;
using Cubes.Core.Base;
using Cubes.Core.Web.ResponseWrapping;
using FluentValidation.AspNetCore;
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
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
            => _configuration = configuration;

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
                .ConfigureApiBehaviorOptions(options =>
                {
                    var builtInFactory = options.InvalidModelStateResponseFactory;
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var logger = context
                            .HttpContext
                            .RequestServices
                            .GetService<ILoggerFactory>()
                            .CreateLogger<Startup>();

                        var sb = new StringBuilder();
                        foreach (var item in context.ModelState)
                        {
                            sb.Append("    ").Append(item.Key).AppendLine(":");
                            foreach (var error in item.Value.Errors)
                                sb.Append("      - ").AppendLine(error.ErrorMessage);
                        }

                        logger.LogWarning("Model validation failed! {action}\r\n" + sb.ToString(),
                            context.ActionDescriptor.DisplayName);

                        return builtInFactory(context);
                    };
                })
                .AddControllersAsServices()
                .AddFluentValidation()
                .AddNewtonsoftJson();

            // Add applications assemblies
            foreach (var asm in assemblies)
            {
                mvcBuilder
                    .AddApplicationPart(asm)
                    .AddControllersAsServices();
            }

            // Setup Cubes
            services.AddCubesWeb(_configuration, mvcBuilder);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var useSsl             = _configuration.GetValue<bool>(CubesConstants.Config_HostUseSSL, false);
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
            app.UseCubesApi(_configuration, env, responseBuilder, loggerFactory, serializerSettings);

            // Routing
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapControllers());
       }
    }
}
