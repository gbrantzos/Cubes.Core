namespace Cubes.Core.Security
{
    public class InternalAdminPassword
    {
        private static readonly string password = PasswordGenerator.GeneratePassword();

        public string Password => password;
    }
}
