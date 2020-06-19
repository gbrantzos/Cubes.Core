using System.Collections.Generic;

namespace Cubes.Core.Security
{
    public class UserDetails
    {
        public int ID { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }

        public List<string> Roles { get; set; } = new List<string>();
    }
}
