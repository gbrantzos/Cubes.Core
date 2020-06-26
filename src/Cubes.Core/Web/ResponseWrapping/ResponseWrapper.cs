using System.Collections.Generic;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Cubes.Core.Base;
using Cubes.Core.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Cubes.Core.Web.ResponseWrapping
{
    // Base on the great article:
    // https://www.codeproject.com/Articles/1248022/ASP-NET-Core-and-Web-API-A-Custom-Wrapper-for-Mana
    // https://www.c-sharpcorner.com/article/asp-net-core-and-web-api-a-custom-wrapper-for-managing-exceptions-and-consiste/

    public class ResponseWrapper
    {
        private readonly static HashSet<string> systemExcluded = new HashSet<string>
        {
            "/api/system/version"
        };
        private readonly RequestDelegate next;
        private readonly ILogger<ResponseWrapper> logger;
        private readonly IApiResponseBuilder responseBuilder;
        private readonly string includePath;
        private readonly string excludePath;

        public ResponseWrapper(RequestDelegate next,
            ILogger<ResponseWrapper> logger,
            IApiResponseBuilder responseBuilder,
            IConfiguration configuration)
        {
            this.next = next;
            this.logger = logger;
            this.responseBuilder = responseBuilder;

            this.includePath = configuration.GetValue(CubesConstants.Config_HostWrapPath, "/api/");
            this.excludePath = configuration.GetValue(CubesConstants.Config_HostWrapPathExclude, "");
        }

        // https://stackoverflow.com/a/47183053/3410871
        public async Task Invoke_New(HttpContext context)
        {
            //Hold on to original body for downstream calls
            Stream originalBody = context.Response.Body;
            try
            {
                string responseBody = null;
                using (var memStream = new MemoryStream())
                {
                    //Replace stream for upstream calls.
                    context.Response.Body = memStream;
                    //continue up the pipeline
                    await next(context);
                    //back from upstream call.
                    //memory stream now hold the response data
                    //reset position to read data stored in response stream
                    memStream.Position = 0;
                    responseBody = new StreamReader(memStream).ReadToEnd();
                }//dispose of previous memory stream.
                 //lets convert responseBody to something we can use
                 // TODO Check for simple string or other...
                var data = JsonConvert.DeserializeObject<dynamic>(responseBody);
                //create your wrapper response and convert to JSON
                var json = new BaseClass
                {
                    data = data,
                    apiVersion = "1.2",
                    otherInfoHere = "here"
                };
                //convert json to a stream
                var buffer = Encoding.UTF8.GetBytes(json.ToString());
                using (var output = new MemoryStream(buffer))
                {
                    await output.CopyToAsync(originalBody);
                }//dispose of output stream
            }
            finally
            {
                //and finally, reset the stream for downstream calls
                context.Response.Body = originalBody;
            }
        }

        public async Task Invoke(HttpContext context)
        {
            // Honor include and exclude paths
            var requestPath = context.Request.Path.Value;
            var shouldSkip = !requestPath.StartsWith(this.includePath) ||
                (!String.IsNullOrEmpty(this.excludePath) && requestPath.StartsWith(this.excludePath)) ||
                systemExcluded.Contains(requestPath);
            if (shouldSkip)
            {
                await this.next(context);
                return;
            }

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
                if (context.Response.IsSuccess())
                {
                    await HandleResponse(body: await ResponseBody(ms),
                       context,
                       context.Response.StatusCode);
                }
                else
                {
                    await HandleErrorResponse(body: await ResponseBody(ms),
                       context,
                       context.Response.StatusCode);
                }
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
                .WithData(bodyObject);

            return WriteToResponse(apiResponse, context);
        }

        private Task HandleErrorResponse(string body, HttpContext context, int statusCode)
        {
            var bodyObject = ConvertBody(body);
            var apiResponse = responseBuilder.Create()
                .WithErrors()
                .WithStatusCode(statusCode);

            if (bodyObject.ToString().IsJson())
            {
                apiResponse.WithData(bodyObject);
            }
            else
            {
                apiResponse.WithMessage(bodyObject.ToString());
            }
            return WriteToResponse(apiResponse, context);
        }

        private Task WriteToResponse(ApiResponse apiResponse, HttpContext context)
        {
            var wrappedBody = apiResponse.ToString();
            context.Response.ContentType = "application/json";
            context.Response.ContentLength = wrappedBody != null ? Encoding.UTF8.GetByteCount(wrappedBody) : 0;
            return context.Response.WriteAsync(wrappedBody);
        }

        private object ConvertBody(string body)
        {
            try
            {
                var toReturn = body.IsJson() ?
                    JsonConvert.DeserializeObject(body) :
                    body;
                return toReturn;
            }
            catch (Exception)
            {
                // Failed to deserialize, return string
                logger.LogWarning($"Failed to deserialize JSON string: {body}");
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

            var wrappedBody = apiResponse.ToString();
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = apiResponse.StatusCode;

            return context.Response.WriteAsync(wrappedBody);
        }

        private class BaseClass
        {
            public string apiVersion { get; set; }
            public string otherInfoHere { get; set; }
            public object data { get; set; }

            public override string ToString() => JsonConvert.SerializeObject(this);
        }
    }

    public static class ResponseWrapperExtensions
    {
        public static IApplicationBuilder UseResponseWrapper(this IApplicationBuilder builder)
            => builder.UseMiddleware<ResponseWrapper>();
    }
}
