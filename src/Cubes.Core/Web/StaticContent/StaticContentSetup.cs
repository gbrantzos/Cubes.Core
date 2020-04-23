using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Cubes.Core.Base;
using Cubes.Core.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

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
                var contentPath = Directory.Exists(item.FileSystemPath) ?
                    item.FileSystemPath :
                    Path.Combine(rootPath, item.FileSystemPath);
                if (!item.RequestPath.StartsWith("/"))
                    item.RequestPath = "/" + item.RequestPath;

                if (!Directory.Exists(contentPath))
                {
                    logger.LogError($"Cannot load Static Content on relative URL '{item.RequestPath}', path does not exist >> {contentPath}!");
                    return app;
                }

                app.Map(new PathString(item.RequestPath),
                    builder =>
                    {
                        logger.LogInformation("Preparing static content listening on '{requestPath}', serving from {contentPath}", item.RequestPath, contentPath);
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
                                await context
                                    .Response
                                    .SendFileAsync(Path.Combine(rootPath, item.FileSystemPath, item.DefaultFile));
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
            var cubesConfig = configuration.GetCubesConfiguration();
            var zipPath     = configuration.GetCubesConfiguration().AdminPath;
            var logger      = loggerFactory.CreateLogger<Content>();

            if (!File.Exists(zipPath))
            {
                logger.LogWarning($"Could not load 'Cubes Management' application from path: {zipPath}");
                return app;
            }
            else
            {
                logger.LogInformation("Serving 'Cubes Management' from {contentPath}, request path '{requestPath}'.", zipPath, "/admin");
            }

            // Currently Compressed FileProvider seems to be broken!
            var useCompressedFileProvider = false;
            IFileProvider fileProvider;

            if (useCompressedFileProvider)
            {
                fileProvider = new CompressedFileProvider(zipPath);

                // Inform for changes on CompressedFileProvider
                var fileName = Path.GetFileName(zipPath);
                var debouncer = new Debouncer();
                Action call = () => logger.LogInformation("File {cubesAdminPath} changed, 'Cubes Management' should be reloaded!", fileName);
                ChangeToken.OnChange(
                    () => fileProvider.Watch(fileName),
                    () => debouncer.Debouce(call));
            }
            else
            {
                var tempFolder = Path.Combine(cubesConfig.TempFolder, "CubesMangement");
                DeployZipOnTemp(tempFolder, zipPath);
                fileProvider = new PhysicalFileProvider(tempFolder);

                // Setup file changes mechanism for zip file
                var fileName = Path.GetFileName(zipPath);
                var pfp = new PhysicalFileProvider(Path.GetDirectoryName(zipPath));
                var debouncer = new Debouncer();
                Action call = () =>
                {
                    DeployZipOnTemp(tempFolder, zipPath);
                    logger.LogInformation($"File {fileName} changed, 'Cubes Management' should be reloaded!");
                };
                ChangeToken.OnChange(
                    () => pfp.Watch(fileName),
                    () => debouncer.Debouce(call));
            }

            var options = new FileServerOptions
            {
                FileProvider       = fileProvider,
                RequestPath        = "",
                EnableDefaultFiles = true,
            };
            options.DefaultFilesOptions.DefaultFileNames.Clear();
            options.DefaultFilesOptions.DefaultFileNames.Add("index.html");
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
                        if (useCompressedFileProvider)
                        {
                            await fileProvider.GetFileInfo("index.html")
                                .CreateReadStream()
                                .CopyToAsync(context.Response.Body);
                        }
                        else
                        {
                            await context.Response
                                .WriteAsync(File.ReadAllText(fileProvider.GetFileInfo("index.html").PhysicalPath));
                        }
                    }
                });
            });

            return app;
        }

        private static void DeployZipOnTemp(string target, string zipPath)
        {
            // Make sure that target folder is empty
            if (Directory.Exists(target))
            {
                Directory.Delete(target, true);
                Directory.CreateDirectory(target);
            }

            // Extract zip
            using var fileStream = new FileStream(zipPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var archive = new ZipArchive(fileStream);
            archive.ExtractToDirectory(target);
        }
    }
}