using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cubes.Core.Security
{
    public interface IRoleRepository
    {
        Task<IEnumerable<Role>> Get();
        Task AddRoles(IEnumerable<Role> roles);
        Task DeleteByCode(IEnumerable<string> toDelete);
        Task Update(IEnumerable<Role> toUpdate);
    }
}
