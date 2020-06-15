using System;
using System.Collections.Generic;
using System.Text;

namespace Cubes.Core.Security
{
    public class UserDetails
    {
        public int ID { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }

        public List<Role> Roles { get; set; } = new List<Role>();
    }
}
