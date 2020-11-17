using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cubes.Core.Commands;

namespace Cubes.Core.Security
{
    public class SaveRolesHandler : RequestHandler<SaveRoles, bool>
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRepository _userRepository;

        public SaveRolesHandler(IRoleRepository roleRepository, IUserRepository userRepository)
        {
            this._roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            this._userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        protected override async Task<bool> HandleInternal(SaveRoles request, CancellationToken cancellationToken)
        {
            var users = await this._userRepository.GetAll();
            var usedRoles = users
                .SelectMany(u => u.Roles)
                .ToList();

            var allRoles = (await _roleRepository.Get()).ToList();
            var missing = allRoles
                .Where(r => !request.Roles.Select(i => i.Code).Contains(r.Code) && !r.IsSystem)
                .Select(r => r.Code)
                .ToList();
            var needed = usedRoles.Where(r => missing.Contains(r)).ToList();
            if (needed.Count > 0)
            {
                var plurarlSuffix = needed.Count == 1 ? String.Empty : "s";
                var isOrAre = needed.Count == 1 ? "is" : "are";
                var roles = String.Join(", ", needed);
                return FailedResponse($"Role{plurarlSuffix} '{roles}' {isOrAre} in use and cannot be deleted!");
            }
            await _roleRepository.DeleteByCode(missing);

            var newRoles = request
                .Roles
                .Where(r => !allRoles.Select(i => i.Code).ToList().Contains(r.Code) && !r.IsSystem)
                .ToList();
            await _roleRepository.AddRoles(newRoles);

            var existing = request
                .Roles
                .Where(r => allRoles.Select(i => i.Code).ToList().Contains(r.Code) && !r.IsSystem)
                .ToList();
            await this._roleRepository.Update(existing);

            MessageToReturn = "Roles saved!";
            return true;
        }
    }
}
