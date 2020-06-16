using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Cubes.Core.Base;
using LiteDB;
using Microsoft.Extensions.Options;

namespace Cubes.Core.Security
{
    // Ideas taken from
    // https://www.meziantou.net/how-to-store-a-password-in-a-web-application.htm

    public class UserRepository : IUserRepository
    {
        private readonly string dbPath;
        private readonly InternalAdminPassword adminPassword;

        public UserRepository(InternalAdminPassword adminPassword, IOptions<CubesConfiguration> configuration)
        {
            this.adminPassword = adminPassword;
            this.dbPath = Path.Combine(configuration.Value.StorageFolder, CubesConstants.Authentication_Persistence);
        }

        public Task<User> GetUser(string userName, string password)
        {
            if (userName == CubesConstants.Authentication_InternalAdmin && password == adminPassword.Password)
            {
                var adminUser = new User
                {
                    ID          = -1,
                    UserName    = CubesConstants.Authentication_InternalAdmin,
                    DisplayName = "Cubes Administrator",
                    Roles       = new List<string> { Role.AdminRole.Code }
                };

                return Task.FromResult(adminUser);
            }

            using var storage = GetStorage();
            var user = storage
                .GetCollection<User>()
                .FindOne(u => u.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));

            var hashedPassword = ComputeHash(user.PasswordSalt + password);
            if (hashedPassword == user.Password)
                return Task.FromResult(user.WithoutPassword());

            return Task.FromResult<User>(null);
        }

        public Task SaveUser(UserDetails userDetails, string password)
        {
            using var storage = GetStorage();
            var userCollection = storage.GetCollection<User>();
            var existing = userCollection
                .FindOne(u => u.UserName.Equals(userDetails.UserName, StringComparison.OrdinalIgnoreCase));
            bool isNew = existing == null;

            var user = isNew ? new User() : existing;
            user.UserName     = userDetails.UserName;
            user.DisplayName  = userDetails.DisplayName;
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
            using var storage = GetStorage();
            var userCollection = storage.GetCollection<User>();
            var existing = userCollection
                .FindOne(u => u.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
                userCollection.Delete(new BsonValue(existing.ID));

            return Task.CompletedTask;
        }

        public Task<IEnumerable<UserDetails>> GetAll()
        {
            using var storage = GetStorage();
            var users = storage
                .GetCollection<User>()
                .FindAll()
                .Select(u => new UserDetails
                {
                    UserName = u.UserName,
                    DisplayName = u.DisplayName,
                    Roles = u.Roles.ToList()
                })
                .ToList();

            return Task.FromResult(users.AsEnumerable());
        }

        private LiteDatabase GetStorage() => new LiteDatabase(dbPath);

        private string ComputeHash(string input)
        {
            var buffer = Encoding.UTF8.GetBytes(input);
            using var hashAlgorithm = HashAlgorithm.Create("SHA256");

            return Convert.ToBase64String(hashAlgorithm.ComputeHash(buffer));
        }
    }
}
