using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cubes.Core.Commands;

namespace Cubes.Core.Security
{
    public class GetRolesHandler : RequestHandler<GetRoles, IEnumerable<Role>>
    {
        private readonly IRoleRepository roleRepository;

        public GetRolesHandler(IRoleRepository roleRepository)
            => this.roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));

        protected override async Task<IEnumerable<Role>> HandleInternal(GetRoles request, CancellationToken cancellationToken)
            => await this.roleRepository.Get();
    }
}
