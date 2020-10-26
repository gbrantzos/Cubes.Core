using System.Net;
using NetTools;

namespace Cubes.Core.Web.IpRestrictions
{
    public interface IIpMatcher
    {
        bool Match(IPAddress ipAddress, string ipRange);
    }

    public class IpMatcher : IIpMatcher
    {
        public bool Match(IPAddress ipAddress, string ipRange)
        {
            var range = IPAddressRange.Parse(ipRange);
            return range.Contains(ipAddress);
        }
    }
}
