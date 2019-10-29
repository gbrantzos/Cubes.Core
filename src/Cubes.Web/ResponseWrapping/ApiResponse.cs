using System;
using Newtonsoft.Json;

namespace Cubes.Web.ResponseWrapping
{
    public class ApiResponse
    {
        public DateTime CreatedAt { get; set; }
        public int StatusCode     { get; set; }
        public string Message     { get; set; }
        public bool HasErrors     { get; set; }
        public object Response    { get; set; }

        public override string ToString()
            => JsonConvert.SerializeObject(this);

        private static ApiResponse CreateResponse(int statusCode, string message, object response, bool hasErrors)
            => new ApiResponse
            {
                CreatedAt  = DateTime.UtcNow,
                StatusCode = statusCode,
                Message    = message,
                Response   = response,
                HasErrors  = hasErrors
            };

        public static ApiResponse Ok(int statusCode, string message, object response)
            => CreateResponse(statusCode, message, response, false);

        public static ApiResponse Ok(string message, object response)
            => Ok(200, message, response);

        public static ApiResponse Ok(object response)
            => Ok(200, "Success", response);
    }
}
