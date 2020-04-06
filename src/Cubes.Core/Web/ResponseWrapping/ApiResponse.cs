using System;
using Cubes.Core.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Cubes.Core.Web.ResponseWrapping
{
    public class ApiResponse
    {
        private static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
        };

        public string Version     { get; set; }
        public DateTime CreatedAt { get; set; }
        public int StatusCode     { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Message     { get; set; }
        public bool HasErrors     { get; set; }
        public object Response    { get; set; }

        public override string ToString()
            => JsonConvert.SerializeObject(this, serializerSettings);

        internal ApiResponse(DateTime createdAt, string version)
        {
            CreatedAt = createdAt;
            Version   = version.ThrowIfEmpty(nameof(Version));
        }

        internal static ApiResponse CreateResponse() => new ApiResponse(DateTime.UtcNow, String.Empty);
    }

    public static class ApiResponseExtensions
    {
        public static ApiResponse HasErrors(this ApiResponse response)
        {
            (response ?? ApiResponse.CreateResponse()).HasErrors = true;
            return response;
        }

        public static ApiResponse WithStatusCode(this ApiResponse apiResponse, int statusCode)
        {
            (apiResponse ?? ApiResponse.CreateResponse()).StatusCode = statusCode;
            return apiResponse;
        }

        public static ApiResponse WithMessage(this ApiResponse apiResponse, string message)
        {
            (apiResponse ?? ApiResponse.CreateResponse()).Message = message;
            return apiResponse;
        }

        public static ApiResponse WithResponse(this ApiResponse apiResponse, object response)
        {
            (apiResponse ?? ApiResponse.CreateResponse()).Response = response;
            return apiResponse;
        }

        public static ApiResponse WithExceptionMessages(this ApiResponse response, Exception ex)
        {
            var exceptionMessages = ex.GetAllMessages();
            (response ?? ApiResponse.CreateResponse()).Message = String.Join(Environment.NewLine, exceptionMessages);
            return response;
        }
    }
}
