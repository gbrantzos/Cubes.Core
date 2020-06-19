using System;
using System.Threading;
using System.Threading.Tasks;
using Cubes.Core.Commands;

namespace Cubes.Core.Security
{
    public class DeleteUserHandler : RequestHandler<DeleteUser, bool>
    {
        private readonly IUserRepository userRepository;

        public DeleteUserHandler(IUserRepository userRepository)
            => this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

        protected override async Task<bool> HandleInternal(DeleteUser request, CancellationToken cancellationToken)
        {
            await userRepository.DeleteUser(request.UserName);

            MessageToReturn = $"User {request.UserName} deleted!";
            return true;
        }
    }
}
