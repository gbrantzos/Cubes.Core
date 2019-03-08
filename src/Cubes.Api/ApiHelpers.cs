using System;
using System.Collections.Generic;
using System.Reflection;
using Cubes.Api.Controllers;
using Cubes.Core.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Cubes.Api
{
    public static class ApiHelpers
    {
        public static void AddCubesApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CubesNext API", Version = "v1" });
                c.EnableAnnotations();
                c.OperationFilter<CustomFilter>();
                //c.
                //c.IncludeXmlComments
                //c.TagActionsBy(api => new List<string> { api.GroupName });
            });
        }

        public static IApplicationBuilder UseCubes(this IApplicationBuilder app)
        {
            app.UseSwagger(c => c.RouteTemplate = "docs/{documentName}/swagger.json");
            app.UseSwaggerUI(c =>
            {
                c.DocExpansion(DocExpansion.None);
                c.SwaggerEndpoint("/docs/v1/swagger.json", "CubesNext API V1");
                c.RoutePrefix = "docs/api";

                //c.GroupActionsBy()
            });

            return app;
        }
    }

    public class CustomFilter : IOperationFilter
    {


        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attr = context.MethodInfo.DeclaringType.GetAttribute<SwaggerCategoryAttribute>();
            var tag = attr?.Prefix;
            if (!String.IsNullOrEmpty(tag))
                operation.Tags = new List<OpenApiTag> {new OpenApiTag { Name = tag, Description = "Core operations" }};
        }
    }
}
