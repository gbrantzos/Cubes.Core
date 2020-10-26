namespace Cubes.Core.Web.IpRestrictions
{
    public class IpRestrictionsMiddlewareOptions
    {
        public bool TurnOff { get; set; }
        public string[] WhiteList { get; set; } = new string[] { };
        public string[] BlackList { get; set; } = new string[] { };

        public int StatusCode { get; set; } = 403;
    }
}
