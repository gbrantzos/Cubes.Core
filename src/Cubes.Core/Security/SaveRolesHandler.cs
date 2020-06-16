using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cubes.Core.Commands;

namespace Cubes.Core.Security
{
    public class SaveRolesHandler : RequestHandler<SaveRoles, bool>
    {
        private readonly IRoleRepository roleRepository;
        private readonly IUserRepository userRepository;

        public SaveRolesHandler(IRoleRepository roleRepository, IUserRepository userRepository)
        {
            this.roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        protected override async Task<bool> HandleInternal(SaveRoles request, CancellationToken cancellationToken)
        {
            var users = await this.userRepository.GetAll();
            var usedRoles = users
                .SelectMany(u => u.Roles)
                .ToList();
            var requestRoles = request
                .Roles
                .Where(r => !r.IsSystem)
                .Select(r => r.Code)
                .ToList();

            var needed = usedRoles.Where(r => !requestRoles.Contains(r)).ToList();
            if (needed.Count > 0)
            {
                var roles = String.Join(", ", needed);
                throw new Exception($"Roles {roles} are in use and cannot be deleted!");
            }

            await this.roleRepository.Save(request.Roles);
            MessageToReturn = "Roles saved!";

            return true;
        }
    }
}
