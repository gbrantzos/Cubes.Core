using System;
using System.Data.Common;

namespace Cubes.Core.DataAccess
{
    public interface IConnectionManager
    {
        /// <summary>
        /// Get a new <see cref="DbConnection"/> instance created using details on DataAccessOptions file.
        /// </summary>
        /// <param name="connectionName">Name of connection on configuration file</param>
        /// <returns>an instance of <see cref="DbConnection"/></returns>
        /// <exception cref="ArgumentException">Thrown when connectionName not found</exception>
        DbConnection GetConnection(string connectionName);

        /// <summary>
        /// Get a new <see cref="DbConnection"/> instance created using given <paramref name="connection"/>.
        /// </summary>
        /// <param name="connection">A <see cref="Connection"/> instance</param>
        /// <returns>an instance of <see cref="DbConnection"/></returns>
        /// <exception cref="ArgumentException">Thrown when connectionName not found</exception>
        DbConnection GetConnection(Connection connection);
    }
}