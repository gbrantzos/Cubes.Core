using Microsoft.AspNetCore.Builder;

namespace Cubes.Core.Web.IpRestrictions
{
    public static class IpRestrictionsMiddlewareHelpers
    {
        public static IApplicationBuilder UseIpRestrictions(this IApplicationBuilder builder)
            => builder.UseMiddleware<IpRestrictionsMiddleware>();
    }
}
