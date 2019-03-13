using System.IO;
using Cubes.Core.Environment;
using Cubes.Core.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace Cubes.Api.StaticContent
{
    public static class StartupExtensions
    {
        public static IApplicationBuilder UseStaticContent(this IApplicationBuilder app,
            ISettingsProvider settingsProvider,
            ICubesEnvironment environment,
            ILoggerFactory loggerFactory)
        {
            var staticContent = settingsProvider.Load<StaticContentSettings>();
            var rootPath = environment.GetFolder(CubesFolderKind.StaticContent);
            var logger = loggerFactory.CreateLogger<Content>();

            foreach (var item in staticContent.Content)
            {
                var contentPath = item.PathIsAbsolute ?
                    item.FileSystemPath :
                    Path.Combine(rootPath, item.FileSystemPath);

                if (!Directory.Exists(contentPath))
                {
                    logger.LogError($"Cannot load Static Content on relative URL '{item.RequestPath}', path does not exist >> {contentPath}!");
                    return app;
                }

                app.Map($"/{item.RequestPath}",
                    builder =>
                    {
                        logger.LogInformation($"Preparing static content '{item.RequestPath}'");
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
                                // Fallback to SPA entry point
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
                FileProvider = new ManifestEmbeddedFileProvider(typeof(StartupExtensions).Assembly, "Resources"),
                RequestPath = "",
                EnableDefaultFiles = true
            };
            fsOptions.DefaultFilesOptions.DefaultFileNames.Add("index.html");
            app.UseFileServer(fsOptions);

            return app;
        }
    }
}