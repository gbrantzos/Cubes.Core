using System.Collections.Generic;
using Cubes.Core.Commands;

namespace Cubes.Core.Security
{
    public class GetUsers : Request<IEnumerable<UserDetails>> { }
}