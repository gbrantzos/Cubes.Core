using System.Data.Common;

namespace Cubes.Core.DataAccess
{
    public interface IDatabaseConnectionManager
    {
        DbConnection GetConnection(string connectionName);
    }
}