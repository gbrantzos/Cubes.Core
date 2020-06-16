using System.Collections.Generic;

namespace Cubes.Core.Security
{
    public class UserDetails
    {
        public string UserName { get; set; }
        public string DisplayName { get; set; }

        public List<string> Roles { get; set; } = new List<string>();
    }
}
