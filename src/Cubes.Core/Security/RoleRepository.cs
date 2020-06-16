using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cubes.Core.Base;
using LiteDB;
using Microsoft.Extensions.Options;

namespace Cubes.Core.Security
{
    public class RoleRepository : IRoleRepository
    {
        private readonly string dbPath;

        public RoleRepository(IOptions<CubesConfiguration> configuration)
        {
            this.dbPath = Path.Combine(configuration.Value.StorageFolder, CubesConstants.Authentication_Persistence);
        }

        public Task<IEnumerable<Role>> Get()
        {
            using var storage = GetStorage();
            var roles = storage
                .GetCollection<Role>()
                .FindAll()
                .ToList();

            var toReturn = Role.SystemRoles.Concat(roles).ToList();
            return Task.FromResult(toReturn.AsEnumerable());
        }

        public Task Save(IEnumerable<Role> roles)
        {
            using var storage = GetStorage();
            var rolesCollection = storage.GetCollection<Role>();
            rolesCollection.DeleteAll();
            rolesCollection.InsertBulk(roles.Where(r => !r.IsSystem));
            rolesCollection.EnsureIndex(r => r.Code, true);

            return Task.CompletedTask;
        }

        private LiteDatabase GetStorage() => new LiteDatabase(dbPath);
    }
}
