using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Cubes.Web.RequestContext
{
    public class Context
    {
        public string ID { get; private set; }
        public string IP { get; private set; }
        public string Url { get; private set; }
        public string Method { get; private set; }
        public string QueryString { get; private set; }
        public DateTime StartedAt { get; private set; }

        public Dictionary<string, object> Data { get; private set; }

        public static Context FromHttpContext(HttpContext httpContext)
            => new Context
                {
                    ID = Guid.NewGuid().ToString(),
                    IP = httpContext.Connection.RemoteIpAddress.ToString(),
                    Url = httpContext.Request.Path,
                    Method = httpContext.Request.Method,
                    QueryString = httpContext.Request.QueryString.Value,
                    StartedAt = DateTime.Now,

                    Data = new Dictionary<string, object>()
                };
    }
}
