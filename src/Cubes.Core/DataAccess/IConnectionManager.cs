using System.Data.Common;

namespace Cubes.Core.DataAccess
{
    public interface IConnectionManager
    {
        DbConnection GetConnection(string connectionName);
    }
}