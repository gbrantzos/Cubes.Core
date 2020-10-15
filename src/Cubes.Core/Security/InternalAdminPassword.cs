namespace Cubes.Core.Security
{
    public class InternalAdminPassword
    {
        private static readonly string _password = PasswordGenerator.GeneratePassword();

        public string Password => _password;
    }
}
