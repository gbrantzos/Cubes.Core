using System;
using System.Collections.Generic;
using System.Reflection;
using Cubes.Web.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Cubes.Web
{
    public static class SwaggerHelpers
    {
        public static void AddCubesSwaggerServices(this IServiceCollection services, IConfiguration configuration)
        {
            var rootFolder = configuration.GetValue<string>("Cubes:RootFolder");
            var appsFolder = configuration.GetValue<string>("Cubes:AppsFolder");
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CubesNext API Documentation", Version = "v1" });
                c.OperationFilter<SwaggerCategoryAsTagFilter>();

                //c.IncludeXmlComments(Path.Combine(rootFolder, "Cubes.Web.xml"));
                //var xmlFiles = cubesEnvironment
                //    .GetLoadedApps()
                //    .Select(i =>
                //    {
                //        var file = Path.Combine(cubesEnvironment.GetAppsFolder(), $"{Path.GetFileNameWithoutExtension(i.File)}.xml");
                //        return File.Exists(file) ? file : String.Empty;
                //    })
                //    .Where(i => !String.IsNullOrEmpty(i))
                    //.ToList();

                // TODO This how?
                //foreach (var file in xmlFiles)
                    //c.IncludeXmlComments(Path.Combine(cubesEnvironment.GetAppsFolder(), file));
            });
        }

        public static IApplicationBuilder UseCubesSwagger(this IApplicationBuilder app)
        {
            app.UseSwagger(c => c.RouteTemplate = "docs/{documentName}/swagger.json");
            app.UseSwaggerUI(c =>
            {
                c.DocExpansion(DocExpansion.List);
                c.SwaggerEndpoint("/docs/v1/swagger.json", "CubesNext API V1");
                c.RoutePrefix = "docs/api";

                c.DisplayRequestDuration();
                c.DocumentTitle = "CubesNext API";
            });

            return app;
        }
    }

    public class SwaggerCategoryAsTagFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attr = context
                .MethodInfo
                .DeclaringType
                .Assembly
                .GetCustomAttribute(typeof(SwaggerCategoryAttribute)) as SwaggerCategoryAttribute;
            var tag = attr?.Category;
            if (!String.IsNullOrEmpty(tag))
                operation.Tags = new List<OpenApiTag> {new OpenApiTag { Name = tag }};
        }
    }
}
