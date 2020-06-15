using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cubes.Core.Security
{
    public interface IUserRepository
    {
        /// <summary>
        /// Get user with given userName and password, or null if user not found.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<User> GetUser(string userName, string password);

        /// <summary>
        /// Save user (insert or update).
        /// </summary>
        /// <param name="userDetails"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task SaveUser(UserDetails userDetails, string password);

        /// <summary>
        /// Delete user.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task DeleteUser(string userName);

        /// <summary>
        /// Get all users.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<UserDetails>> GetAll();
    }
}
