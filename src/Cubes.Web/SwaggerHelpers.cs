using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cubes.Core.Environment;
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
            services.AddSwaggerGen(c =>
            {
                var cubesConfig = configuration.GetCubesConfiguration();
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Cubes API Documentation", Version = "v1" });
                c.OperationFilter<SwaggerCategoryAsTagFilter>();

                var swaggerFiles = cubesConfig
                    .SwaggerFiles
                    .Where(File.Exists)
                    .ToList();
                foreach (var xmlfile in swaggerFiles)
                    c.IncludeXmlComments(xmlfile);
            });
        }

        public static IApplicationBuilder UseCubesSwagger(this IApplicationBuilder app)
        {
            app.UseSwagger(c => c.RouteTemplate = "docs/{documentName}/swagger.json");
            app.UseSwaggerUI(c =>
            {
                c.InjectStylesheet("https://raw.githubusercontent.com/ostranme/swagger-ui-themes/develop/themes/3.x/theme-muted.css");
                c.DocExpansion(DocExpansion.List);
                c.SwaggerEndpoint("/docs/v1/swagger.json", "Cubes API V1");
                c.RoutePrefix = "docs/api";

                c.DisplayRequestDuration();
                c.DocumentTitle = "Cubes API";
            });
            app.UseReDoc(c =>
            {
                c.RoutePrefix = "docs/api-docs";
                c.SpecUrl = "/docs/v1/swagger.json";
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
