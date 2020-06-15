using System;
using System.Threading;
using System.Threading.Tasks;
using Cubes.Core.Commands;

namespace Cubes.Core.Security
{
    public class AuthenticateUserHandler : RequestHandler<AuthenticateUser, AuthenticateUserResponse>
    {
        private readonly TokenGenerator tokenGenerator;
        private readonly IUserRepository userRepository;

        public AuthenticateUserHandler(TokenGenerator tokenGenerator, IUserRepository userRepository)
        {
            this.tokenGenerator = tokenGenerator;
            this.userRepository = userRepository;
        }

        protected override async Task<AuthenticateUserResponse> HandleInternal(AuthenticateUser request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetUser(request.UserName, request.Password);
            if (user == null)
                throw new InvalidUserOrPasswordException("Invalid user name or password!") { UserName = request.UserName };

            var token = tokenGenerator.GenerateToken(user);
            MessageToReturn = $"Successfully authenticated user {request.UserName}";

            return new AuthenticateUserResponse(user, token);
        }
    }

    public class InvalidUserOrPasswordException : Exception
    {
        public string UserName { get; set; }

        public InvalidUserOrPasswordException() : base() { }

        public InvalidUserOrPasswordException(string message) : base(message) { }

        public InvalidUserOrPasswordException(string message, Exception innerException) : base(message, innerException) { }
    }
}
