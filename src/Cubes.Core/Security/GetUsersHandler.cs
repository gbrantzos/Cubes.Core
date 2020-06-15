using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cubes.Core.Commands;

namespace Cubes.Core.Security
{
    public class GetUsersHandler : RequestHandler<GetUsers, IEnumerable<UserDetails>>
    {
        private readonly IUserRepository userRepository;

        public GetUsersHandler(IUserRepository userRepository)
            => this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

        protected override async Task<IEnumerable<UserDetails>> HandleInternal(GetUsers request, CancellationToken cancellationToken)
            => await this.userRepository.GetAll();
    }
}
