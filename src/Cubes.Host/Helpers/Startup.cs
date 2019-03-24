using Cubes.Api;
using Cubes.Core;
using Cubes.Core.Environment;
using Cubes.Core.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Cubes.Host.Helpers
{
    public class Startup
    {
        private readonly ILoggerFactory loggerFactory;
        private readonly IConfiguration configuration;
        private readonly ICubesEnvironment cubesEnvironment;
        private readonly bool enableCompression;

        public Startup(IConfiguration configuration, ICubesEnvironment cubesEnvironment, ILoggerFactory loggerFactory)
        {
            this.configuration = configuration;
            this.cubesEnvironment = cubesEnvironment;
            this.loggerFactory = loggerFactory;

            this.enableCompression = configuration.GetValue<bool>("enableCompression", true);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Include;
                })
                .AddApplicationPart(typeof(CubesApiModule).Assembly)
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddCubesCore(configuration);
            services.AddCubesApi(cubesEnvironment, enableCompression);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ISettingsProvider settingsProvider)
        {
            var useSSL = configuration.GetValue<bool>("useSSL", false);
            if (useSSL)
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            // Should be called as soon as possible.
            app.UseCubesApi(settingsProvider,
                cubesEnvironment,
                loggerFactory,
                enableCompression);

            app.UseMvc();

            // Finally start Cubes
            cubesEnvironment.Start(serviceProvider: app.ApplicationServices);
        }
    }
}
