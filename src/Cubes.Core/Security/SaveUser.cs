using Cubes.Core.Commands;

namespace Cubes.Core.Security
{
    public class SaveUser : Request<bool>
    {
        public UserDetails UserDetails { get; set; } = new UserDetails();

        /// <summary>
        /// Password can be null. In this case only user details are saved.
        /// </summary>
        public string Password { get; set; }
    }
}
