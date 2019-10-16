using System.IO;
using Cubes.Core.Environment;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace Cubes.Web.StaticContent
{
    public static class StaticContentSetup
    {
        public static IApplicationBuilder UseStaticContent(this IApplicationBuilder app,
            IConfiguration configuration,
            ILoggerFactory loggerFactory)
        {
            var settings = configuration
                .GetSection(nameof(StaticContentSettings))
                .Get<StaticContentSettings>();
            var rootPath = configuration.GetCubesConfiguration().StaticContentFolder;
            var logger   = loggerFactory.CreateLogger<Content>();

            if (settings == null)
                return app;

            foreach (var item in settings.Content)
            {
                var contentPath = item.PathIsAbsolute ?
                    item.FileSystemPath :
                    Path.Combine(rootPath, item.FileSystemPath);

                if (!Directory.Exists(contentPath))
                {
                    logger.LogError($"Cannot load Static Content on relative URL '{item.RequestPath}', path does not exist >> {contentPath}!");
                    return app;
                }

                app.Map(new PathString(item.RequestPath),
                    builder =>
                    {
                        logger.LogInformation($"Preparing static content listening on '{item.RequestPath}', serving from {contentPath}");
                        var fsOptions = new FileServerOptions
                        {
                            FileProvider = new PhysicalFileProvider(contentPath),
                            RequestPath = "",
                            EnableDefaultFiles = true
                        };
                        fsOptions.DefaultFilesOptions.DefaultFileNames.Add(item.DefaultFile);
                        fsOptions.StaticFileOptions.ServeUnknownFileTypes = item.ServeUnknownFileTypes;
                        builder.UseFileServer(fsOptions);

                        builder.Use(async (context, next) =>
                        {
                            await next();
                            var fullRequest = context.Request.PathBase.Value + context.Request.Path.Value;
                            if (context.Response.StatusCode == 404 && !Path.HasExtension(fullRequest))
                            {
                                // Fall back to SPA entry point
                                await context.Response.SendFileAsync(Path.Combine(rootPath, item.FileSystemPath, item.DefaultFile));
                            }
                        });
                    });
            }

            return app;
        }

        public static IApplicationBuilder UseHomePage(this IApplicationBuilder app)
        {
            var fsOptions = new FileServerOptions
            {
                FileProvider = new ManifestEmbeddedFileProvider(typeof(StaticContentSetup).Assembly, "Resources"),
                RequestPath = "",
                EnableDefaultFiles = true
            };
            fsOptions.DefaultFilesOptions.DefaultFileNames.Add("index.html");
            app.UseFileServer(fsOptions);

            return app;
        }
    }
}