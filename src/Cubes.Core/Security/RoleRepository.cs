using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;

namespace Cubes.Core.Security
{
    public class RoleRepository : IRoleRepository
    {
        private readonly SecurityStorage _storage;

        public RoleRepository(SecurityStorage storage) => _storage = storage;

        public Task<IEnumerable<Role>> Get()
        {
            var roles = _storage.Roles
                .FindAll()
                .ToList();

            var toReturn = Role.SystemRoles.Concat(roles).ToList();
            return Task.FromResult(toReturn.AsEnumerable());
        }

        public Task Update(IEnumerable<Role> toUpdate)
        {
            var rolesCollection = _storage.Roles;
            var existing = rolesCollection.FindAll().ToList();

            foreach (var role in existing)
            {
                var r = toUpdate.FirstOrDefault(r => r.Code == role.Code);
                if (r != null)
                {
                    rolesCollection.DeleteMany(i => i.Code == r.Code);
                    rolesCollection.Insert(r);
                }
            }
            rolesCollection.EnsureIndex(r => r.Code, true);

            return Task.CompletedTask;
        }

        public Task DeleteByCode(IEnumerable<string> toDelete)
        {
            var rolesCollection = _storage.Roles;
            rolesCollection.DeleteMany(r => toDelete.Contains(r.Code));

            return Task.CompletedTask;
        }

        public Task AddRoles(IEnumerable<Role> roles)
        {
            var rolesCollection = _storage.Roles;
            rolesCollection.InsertBulk(roles);
            rolesCollection.EnsureIndex(r => r.Code, true);

            return Task.CompletedTask;
        }
    }
}
