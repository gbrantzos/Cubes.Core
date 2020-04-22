using System;
using System.IO;
using System.Linq;
using Cubes.Core.Base;
using Cubes.Core.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using OfficeOpenXml.Packaging.Ionic.Zip;

namespace Cubes.Core.Web.StaticContent
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
            var rootPath = configuration.GetCubesConfiguration().ContentFolder;
            var logger   = loggerFactory.CreateLogger<Content>();

            if (settings == null)
                return app;

            foreach (var item in settings.Content.Where(c => c.Active).ToList())
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
                            FileProvider       = new PhysicalFileProvider(contentPath),
                            RequestPath        = "",
                            EnableDefaultFiles = true
                        };
                        fsOptions.DefaultFilesOptions.DefaultFileNames.Clear();
                        fsOptions.DefaultFilesOptions.DefaultFileNames.Add(item.DefaultFile);
                        fsOptions.StaticFileOptions.ServeUnknownFileTypes = item.ServeUnknownFileTypes;
                        builder.UseFileServer(fsOptions);

                        builder.Use(async (context, next) =>
                        {
                            await next();
                            var fullRequest = context.Request.PathBase.Value + context.Request.Path.Value;
                            if (context.Response.StatusCode == 404
                                && !Path.HasExtension(fullRequest)
                                && !Directory.Exists(fullRequest))
                            {
                                // Fall back to SPA entry point
                                context.Response.StatusCode = 200;
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
                FileProvider       = new ManifestEmbeddedFileProvider(typeof(StaticContentSetup).Assembly, "Web/Resources"),
                RequestPath        = "",
                EnableDefaultFiles = true
            };
            fsOptions.DefaultFilesOptions.DefaultFileNames.Add("index.html");
            app.UseFileServer(fsOptions);

            return app;
        }

        public static IApplicationBuilder UseAdminPage(this IApplicationBuilder app,
            IConfiguration configuration,
            ILoggerFactory loggerFactory)
        {
            var zipPath = configuration.GetCubesConfiguration().AdminPath;
            var logger  = loggerFactory.CreateLogger<Content>();

            if (!File.Exists(zipPath))
            {
                logger.LogWarning($"Could not load 'Cubes Management' application from path: {zipPath}");
                return app;
            }
            else
            {
                logger.LogInformation($"Serving 'Cubes Management' from {zipPath}, request path '/admin'.");
            }

            var zfs     = new CompressedFileProvider(zipPath);
            var options = new FileServerOptions
            {
                FileProvider       = zfs,
                RequestPath        = "",
                EnableDefaultFiles = true,
            };
            options.DefaultFilesOptions.DefaultFileNames.Clear();
            options.DefaultFilesOptions.DefaultFileNames.Add("index.html");
            var fileName = Path.GetFileName(zipPath);
            var debouncer = new Debouncer();
            Action call = () => logger.LogInformation($"File {fileName} changed, 'Cubes Management' should be reloaded!");
            ChangeToken.OnChange(
                () => zfs.Watch(fileName),
                () => debouncer.Debouce(call));

            app.Map(new PathString("/admin"), builder =>
            {
                builder.UseFileServer(options);
                builder.Use(async (context, next) =>
                {
                    await next();
                    var fullRequest = context.Request.PathBase.Value + context.Request.Path.Value;
                    if (context.Response.StatusCode == 404
                        && !Path.HasExtension(fullRequest)
                        && !Directory.Exists(fullRequest))
                    {
                        // Fall back to SPA entry point
                        context.Response.StatusCode = 200;
                        await zfs.GetFileInfo("index.html")
                            .CreateReadStream()
                            .CopyToAsync(context.Response.Body);
                    }
                });
            });
            app.UseFileServer(options);

            return app;
        }
    }
}