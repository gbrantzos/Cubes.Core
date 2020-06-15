using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cubes.Core.Security
{
    public interface IRoleRepository
    {
        Task<IEnumerable<Role>> Get();
        Task Save(IEnumerable<Role> roles);
    }
}
