using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Cubes.Web.ResponseWrapping
{
    public class ResponseWrapper
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ResponseWrapper> logger;

        public ResponseWrapper(RequestDelegate next, ILogger<ResponseWrapper> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            // Keep track of original response body
            var originalBody = context.Response.Body;
            context.Request.EnableBuffering();
            using (var ms = new MemoryStream())
            {
                try
                {
                    // Capture response
                    context.Response.Body = ms;

                    await this.next(context);

                    // Restore response body
                    context.Response.Body = originalBody;

                    if (context.Response.StatusCode >= 200 && context.Response.StatusCode <= 299)
                    {
                        // Get actual response body
                        ms.Seek(0, SeekOrigin.Begin);

                        var buffer = new byte[Convert.ToInt32(context.Request.ContentLength)];
                        await context.Request.Body.ReadAsync(buffer, 0, buffer.Length);
                        //var actualBody = await new StreamReader(ms, Encoding.UTF8).ReadToEndAsync();
                        var actualBody = Encoding.UTF8.GetString(buffer);
                        context.Request.Body.Seek(0, SeekOrigin.Begin);

                        // Get actual response object

                        // TODO Is this safe?
                        var jsonStart = new[] { "[", "{" };
                        if (jsonStart.Contains(actualBody.Substring(0, 1)))
                        {
                            var resultObj = JsonConvert.DeserializeObject(actualBody);

                            // Wrap ...
                            var apiResponse = ApiResponse.Ok(resultObj);

                            // Write resultObj to response
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync(apiResponse.ToString());
                        }
                        else
                            await context.Response.WriteAsync(actualBody);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ex.Message);

                    // TODO Must be something configurable!
                    int code = (int)System.Net.HttpStatusCode.InternalServerError;
                    context.Response.StatusCode = code;
                    await context.Response.WriteAsync(ex.ToString());

                    // Restore...
                    ms.Seek(0, SeekOrigin.Begin);
                    await ms.CopyToAsync(originalBody);
                }
            }
        }
    }

    public static class ResponseWrapperExtensions
    {
        public static IApplicationBuilder UseResponseWrapper(this IApplicationBuilder builder)
            => builder.UseMiddleware<ResponseWrapper>();
    }
}
