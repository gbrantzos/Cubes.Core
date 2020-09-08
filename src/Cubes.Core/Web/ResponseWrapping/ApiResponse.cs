using System;
using Cubes.Core.Utilities;
using Newtonsoft.Json;

namespace Cubes.Core.Web.ResponseWrapping
{
    public class ApiResponse
    {
        public string RequestID   { get; set; }
        public string Version     { get; set; }
        public DateTime CreatedAt { get; set; }
        public int StatusCode     { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Message     { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Data        { get; set; }

        internal ApiResponse(DateTime createdAt, string version, string requestID)
        {
            CreatedAt = createdAt;
            Version   = version.ThrowIfEmpty(nameof(Version));
            RequestID = requestID;
        }

        internal static ApiResponse CreateResponse(string requestID)
            => new ApiResponse(DateTime.UtcNow, string.Empty, requestID);
    }

    public static class ApiResponseExtensions
    {
        public static ApiResponse WithStatusCode(this ApiResponse apiResponse, int statusCode)
        {
            apiResponse.StatusCode = statusCode;
            return apiResponse;
        }

        public static ApiResponse WithMessage(this ApiResponse apiResponse, string message)
        {
            apiResponse.Message = message;
            return apiResponse;
        }

        public static ApiResponse WithData(this ApiResponse apiResponse, object data)
        {
            apiResponse.Data = data;
            return apiResponse;
        }
    }
}
