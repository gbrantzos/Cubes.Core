using System;
using System.Threading;
using System.Threading.Tasks;
using Cubes.Core.Commands;

namespace Cubes.Core.Security
{
    public class SaveUserHandler : RequestHandler<SaveUser, bool>
    {
        private readonly IUserRepository userRepository;

        public SaveUserHandler(IUserRepository userRepository)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        protected override async Task<bool> HandleInternal(SaveUser request, CancellationToken cancellationToken)
        {
            await userRepository.SaveUser(request.UserDetails, request.Password);

            MessageToReturn = $"User details changed for {request.UserDetails.DisplayName}";
            return true;
        }
    }
}
