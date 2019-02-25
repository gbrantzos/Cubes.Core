using Cubes.Core.Commands;
using Cubes.Core.Environment;
using Cubes.Core.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Cubes.Host
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSingleton<ISettingsProvider>(s => new JsonFilesSettingsProvider(s.GetService<ICubesEnvironment>().GetSettingsFolder()));
            services.AddScoped<ServiceFactory>(p => p.GetService);
            services.AddScoped<ICommandBus, CommandBus>();

            services.AddScoped(typeof(ICommandHandler<ACommnad, AResult>), typeof(AHandler));
            services.AddScoped(typeof(ICommandBusMiddleware<,>), typeof(LoggingMiddleware<,>));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();

            app.UseFileServer();
        }
    }


    public class ACommnad : ICommand<AResult> { }
    public class AResult { }
    public class AHandler : ICommandHandler<ACommnad, AResult>
    {
        public AResult Handle(ACommnad command)
        {
            return new AResult();
        }
    }

    public class LoggingMiddleware<TCommand, TResult> : ICommandBusMiddleware<TCommand, TResult>
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
