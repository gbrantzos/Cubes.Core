using System;
using System.Threading;
using System.Threading.Tasks;
using Cubes.Core.Commands;

namespace Cubes.Core.Security
{
    public class ResetUserPasswordHandler : RequestHandler<ResetUserPassword, bool>
    {
        private readonly IUserRepository userRepository;

        public ResetUserPasswordHandler(IUserRepository userRepository)
            => this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

        protected override async Task<bool> HandleInternal(ResetUserPassword request, CancellationToken cancellationToken)
        {
            var user = await this.userRepository.GetUser(request.UserName, request.OldPassword);
            if (user == null)
                throw new InvalidUserOrPasswordException("Invalid user name or password!") { UserName = request.UserName };

            await this.userRepository.SaveUser(user.UserDetails(), request.NewPassword);
            MessageToReturn = $"Successfully changed password for user {user.DisplayName}";

            return true;
        }
    }
}
