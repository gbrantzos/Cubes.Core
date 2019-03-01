using Cubes.Core.Commands;
using Cubes.Core.Environment;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;

namespace Cubes.Host
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
            => Configuration = configuration;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddCubes(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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

            app.Map("/wms",
                builder =>
                {
                    var path = @"C:\Code\CubesNext\Core\src\Cubes.Host\bin\Debug\netcoreapp2.2\StaticContent";
                    builder.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(Path.Combine(path, "wms")) });
                    builder.UseDefaultFiles(new DefaultFilesOptions { DefaultFileNames = new[] { "index.html" } });
                    builder.Use(async (context, next) =>
                    {
                        await next();
                        var fullRequest = context.Request.PathBase.Value + context.Request.Path.Value;
                        if (context.Response.StatusCode == 404 && !Path.HasExtension(fullRequest))
                        {
                            context.Request.Path = "/wms/index.html";
                            await next();
                        }
                    });
                });

            app.UseMvc();

            // Default home content
            // var re = new EmbeddedResourceFileSystem();
            app.UseFileServer();
        }
    }


    public class ACommnad : ICommand<AResult> { }
    public class AResult : BaseCommandResult { }
    public class AHandler : ICommandHandler<ACommnad, AResult>
    {
        public AResult Handle(ACommnad command)
        {
            return new AResult();
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
