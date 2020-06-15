using System.Collections.Generic;
using Cubes.Core.Commands;

namespace Cubes.Core.Security
{
    public class SaveRoles : Request<bool>
    {
        public IEnumerable<Role> Roles { get; set; }
    }
}
