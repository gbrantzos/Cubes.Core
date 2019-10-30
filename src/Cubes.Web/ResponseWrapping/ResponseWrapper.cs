using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Cubes.Web.ResponseWrapping
{
    // Base on the great article:
    // https://www.codeproject.com/Articles/1248022/ASP-NET-Core-and-Web-API-A-Custom-Wrapper-for-Mana
    // https://www.c-sharpcorner.com/article/asp-net-core-and-web-api-a-custom-wrapper-for-managing-exceptions-and-consiste/

    public class ResponseWrapper
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ResponseWrapper> logger;
        private readonly IApiResponseBuilder responseBuilder;

        public ResponseWrapper(RequestDelegate next, ILogger<ResponseWrapper> logger, IApiResponseBuilder responseBuilder)
        {
            this.next = next;
            this.logger = logger;
            this.responseBuilder = responseBuilder;
        }

        public async Task Invoke(HttpContext context)
        {
            // Keep track of original response body
            var originalBody = context.Response.Body;

            using var ms = new MemoryStream();
            try
            {
                // Capture response
                context.Response.Body = ms;
                await this.next(context);

                // Restore response body
                context.Response.Body = originalBody;

                // If Success, wrap response, else restore
                if (context.Response.IsSuccess())
                    await HandleResponse(body: await ResponseBody(ms),
                        context,
                        context.Response.StatusCode);
                else
                    await Restore(ms, originalBody);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);

                await HandleException(context, ex);
                await Restore(ms, originalBody);
            }
        }

        // Helpers
        private Task Restore(Stream from, Stream originalStream)
        {
            from.Seek(0, SeekOrigin.Begin);
            return from.CopyToAsync(originalStream);
        }

        private Task HandleResponse(string body, HttpContext context, int statusCode)
        {
            var bodyObject = ConvertBody(body);
            var apiResponse = responseBuilder.Create()
                .WithStatusCode(statusCode)
                .WithResponse(bodyObject);

            var wrappedBody = apiResponse.ToString();
            context.Response.ContentType = "application/json";
            context.Response.ContentLength = wrappedBody != null ? Encoding.UTF8.GetByteCount(wrappedBody) : 0;
            return context.Response.WriteAsync(wrappedBody);
        }

        private object ConvertBody(string body)
        {
            try
            {
                return JsonConvert.DeserializeObject(body);
            }
            catch (Exception)
            {
                // Failed to deserialize, return string
                return body;
            }
        }

        private async Task<string> ResponseBody(Stream response)
        {
            response.Seek(0, SeekOrigin.Begin);
            var bodyRaw = await new StreamReader(response).ReadToEndAsync();
            response.Seek(0, SeekOrigin.Begin);

            return bodyRaw;
        }

        private Task HandleException(HttpContext context, Exception ex)
        {
            var apiResponse = responseBuilder.Create()
                .WithStatusCode((int)HttpStatusCode.InternalServerError)
                .WithExceptionMessages(ex);

            var wrappedBody              = apiResponse.ToString();
            context.Response.ContentType = "application/json";
            context.Response.StatusCode  = apiResponse.StatusCode;

            return context.Response.WriteAsync(wrappedBody);
        }
    }

    public static class ResponseWrapperExtensions
    {
        public static IApplicationBuilder UseResponseWrapper(this IApplicationBuilder builder)
            => builder.UseMiddleware<ResponseWrapper>();
    }
}
