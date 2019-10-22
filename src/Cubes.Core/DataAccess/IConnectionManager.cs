using System;
using System.Data.Common;

namespace Cubes.Core.DataAccess
{
    public interface IConnectionManager
    {
        /// <summary>
        /// Get a new <see cref="DbConnection"/> instance created using details on DataAccessSettings file.
        /// </summary>
        /// <param name="connectionName">Name of connection on settings file</param>
        /// <returns>an instance of <see cref="DbConnection"/></returns>
        /// <exception cref="ArgumentException">Thrown when connectionName not found</exception>
        DbConnection GetConnection(string connectionName);
    }
}