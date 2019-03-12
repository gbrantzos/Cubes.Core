using System;
using Cubes.Api;
using Cubes.Api.StaticContent;
using Cubes.Core.Commands;
using Cubes.Core.Environment;
using Cubes.Core.Jobs;
using Cubes.Core.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cubes.Host.Helpers
{
    public class Startup
    {
        private readonly ILoggerFactory loggerFactory;
        private readonly IConfiguration configuration;
        private readonly ICubesEnvironment cubesEnvironment;

        public Startup(IConfiguration configuration, ICubesEnvironment cubesEnvironment, ILoggerFactory loggerFactory)
        {
            this.configuration    = configuration;
            this.cubesEnvironment = cubesEnvironment;
            this.loggerFactory    = loggerFactory;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddApplicationPart(typeof(SwaggerHelpers).Assembly);
            services.AddCubesSwaggerServices(cubesEnvironment);
            services.AddCubesCoreServices(configuration);
            services.AddCubesApiServices();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ISettingsProvider settingsProvider,
            ICubesEnvironment cubesEnvironment,
            IJobScheduler jobScheduler,
            IApplicationLifetime applicationLifetime)
        {
            var useSSL = configuration.GetValue<bool>("useSSL", false);
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
            {
                if (useSSL)
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
            }
            if (useSSL)
                app.UseHttpsRedirection();

            app.UseCubesContextProvider();

            app.UseMvc();
            app.UseCubesSwagger();
            app.UseCubesStaticContent(settingsProvider,
                cubesEnvironment,
                loggerFactory.CreateLogger<Content>());
            app.UseCubesHomePage();

            jobScheduler.StartScheduler();
            applicationLifetime.ApplicationStopping.Register(() => jobScheduler.StopScheduler());
        }
    }


    public class LoggingMiddleware<TCommand, TResult> : ICommandBusMiddleware<TCommand, TResult> where TResult : ICommandResult
    {
        public TResult Execute(TCommand command, CommandHandlerDelegate<TResult> next)
        {
            Console.WriteLine("Before execution");
            var res = next();
            Console.WriteLine("After execution");

            return res;
        }
    }
}
