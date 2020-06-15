using System.Collections.Generic;
using Cubes.Core.Commands;

namespace Cubes.Core.Security
{
    public class GetRoles : Request<IEnumerable<Role>> { }
}
