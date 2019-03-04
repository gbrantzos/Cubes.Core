using System;
using Cubes.Api;
using Cubes.Api.StaticContent;
using Cubes.Core.Commands;
using Cubes.Core.Environment;
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
        public Startup(IConfiguration configuration)
            => Configuration = configuration;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddApplicationPart(typeof(ApiInfo).Assembly);
            services.AddCubes(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ISettingsProvider settingsProvider,
            ICubesEnvironment cubesEnvironment,
            ILoggerFactory loggerFactory)
        {
            var useSSL = Configuration.GetValue<bool>("useSSL", false);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                if (useSSL)
                    app.UseHsts();
            }

            if (useSSL)
                app.UseHttpsRedirection();

            app.UseStaticContent(settingsProvider,
                cubesEnvironment,
                loggerFactory.CreateLogger<Content>());
            app.UseMvc();

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
