using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Cubes.Core.Base;
using LiteDB;

namespace Cubes.Core.Security
{
    // Ideas taken from
    // https://www.meziantou.net/how-to-store-a-password-in-a-web-application.htm

    public class UserRepository : IUserRepository
    {
        private readonly InternalAdminPassword _adminPassword;
        private readonly SecurityStorage _storage;

        public UserRepository(InternalAdminPassword adminPassword, SecurityStorage storage)
        {
            _adminPassword = adminPassword;
            _storage = storage;
        }

        public Task<User> GetUser(string userName, string password)
        {
            if (userName == CubesConstants.Authentication_InternalAdmin && password == _adminPassword.Password)
            {
                var adminUser = new User
                {
                    ID          = -1,
                    UserName    = CubesConstants.Authentication_InternalAdmin,
                    DisplayName = "Cubes Administrator",
                    Email       = "admin@cubes.local",
                    Roles       = new List<string> { Role.AdminRole.Code }
                };

                return Task.FromResult(adminUser);
            }

            var user = _storage.Users
                .FindOne(u => u.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
            if (user == null)
                return Task.FromResult<User>(null);

            var hashedPassword = ComputeHash(user.PasswordSalt + password);
            if (hashedPassword == user.Password)
                return Task.FromResult(user.WithoutPassword());

            return Task.FromResult<User>(null);
        }

        public Task SaveUser(UserDetails userDetails, string password)
        {
            var userCollection = _storage.Users;
            var existing = userCollection
                .FindOne(u => u.ID == userDetails.ID);
            bool isNew = existing == null;

            var user = isNew ? new User() : existing;
            user.UserName     = userDetails.UserName;
            user.DisplayName  = userDetails.DisplayName;
            user.Email        = userDetails.Email;
            user.Roles        = userDetails.Roles.ToList();
            if (!String.IsNullOrEmpty(password))
            {
                user.PasswordSalt = PasswordGenerator.GeneratePassword();
                user.Password = ComputeHash(user.PasswordSalt + password);
            }
            userCollection.Upsert(user);
            userCollection.EnsureIndex(u => u.UserName, true);

            return Task.CompletedTask;
        }

        public Task DeleteUser(string userName)
        {
            var userCollection = _storage.Users;
            var existing = userCollection
                .FindOne(u => u.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
                userCollection.Delete(new BsonValue(existing.ID));

            return Task.CompletedTask;
        }

        public Task<IEnumerable<UserDetails>> GetAll()
        {
            var users = _storage.Users
                .FindAll()
                .Select(u => u.UserDetails())
                .ToList();

            return Task.FromResult(users.AsEnumerable());
        }

        private string ComputeHash(string input)
        {
            var buffer = Encoding.UTF8.GetBytes(input);
            using var hashAlgorithm = HashAlgorithm.Create("SHA256");

            return Convert.ToBase64String(hashAlgorithm.ComputeHash(buffer));
        }
    }
}
