using System.Collections.Generic;
using System.Linq;

namespace Cubes.Core.Security
{
    public class User
    {
        public int ID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public string DisplayName { get; set; }

        public List<string> Roles { get; set; } = new List<string>();
    }

    public class Role
    {
        public static Role AdminRole = new Role { Code = "admin", Description = "Administrator", IsSystem = true };
        public static IEnumerable<Role> SystemRoles = new Role[] { AdminRole };

        public string Code { get; set; }
        public string Description { get; set; }
        public bool IsSystem { get; set; }
    }

    public static class UserExtensions
    {
        public static User WithoutPassword(this User user)
            => new User
            {
                ID          = user.ID,
                UserName    = user.UserName,
                DisplayName = user.DisplayName,
                Roles       = user.Roles.ToList()
            };

        public static UserDetails UserDetails(this User user)
            => new UserDetails
            {
                UserName    = user.UserName,
                DisplayName = user.DisplayName,
                Roles       = user.Roles.ToList()
            };
    }
}
