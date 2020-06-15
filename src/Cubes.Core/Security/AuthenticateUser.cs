using Cubes.Core.Commands;

namespace Cubes.Core.Security
{
    public class AuthenticateUser : Request<AuthenticateUserResponse>
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
