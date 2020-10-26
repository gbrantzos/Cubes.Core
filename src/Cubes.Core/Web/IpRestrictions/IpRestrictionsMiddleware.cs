using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Cubes.Core.Web.IpRestrictions
{
    public class IpRestrictionsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IIpMatcher _ipMatcher;

        public IpRestrictionsMiddleware(RequestDelegate next,
            IIpMatcher ipMatcher)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _ipMatcher = ipMatcher;
        }

        public async Task InvokeAsync(HttpContext context,
            IOptionsSnapshot<IpRestrictionsMiddlewareOptions> optionsSnapshot)
        {
            var options = optionsSnapshot.Value;

            // Get client address
            var clientIPAddress = context.Connection.RemoteIpAddress;

            // Check if IP should be blocked
            if (ShouldBlock(clientIPAddress, options))
            {
                context.Response.StatusCode = options.StatusCode;
                await context.Response.WriteAsync("Unauthorized access!");
                return;
            }

            // If we reach here continue
            await _next(context);
        }

        private bool ShouldBlock(IPAddress ipAddress, IpRestrictionsMiddlewareOptions options)
        {
            if (options.TurnOff)
                return false;

            // Check on whitelist
            var foundInWhiteList = false;
            foreach (var range in options.WhiteList)
            {
                if (_ipMatcher.Match(ipAddress, range))
                {
                    foundInWhiteList = true;
                    break;
                }
            }
            if (!foundInWhiteList)
                return true;

            // Check on blacklist
            var foundInBlackList = false;
            foreach (var range in options.BlackList)
            {
                if (_ipMatcher.Match(ipAddress, range))
                {
                    foundInBlackList = true;
                    break;
                }
            }
            if (foundInBlackList)
                return true;

            // Looks OK
            return false;
        }
    }
}
