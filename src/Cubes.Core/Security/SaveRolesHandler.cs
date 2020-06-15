using System;
using System.Threading;
using System.Threading.Tasks;
using Cubes.Core.Commands;

namespace Cubes.Core.Security
{
    public class SaveRolesHandler : RequestHandler<SaveRoles, bool>
    {
        private readonly IRoleRepository roleRepository;

        public SaveRolesHandler(IRoleRepository roleRepository)
            => this.roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));

        protected override async Task<bool> HandleInternal(SaveRoles request, CancellationToken cancellationToken)
        {
            await this.roleRepository.Save(request.Roles);
            MessageToReturn = "Roles saved!";

            return true;
        }
    }
}
