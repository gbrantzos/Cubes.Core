using Cubes.Core.Settings;
using System.Collections.Generic;

namespace Cubes.Core.DataAccess
{
    [SettingsPrefix("Core")]
    public class DataAccessSettings
    {
        public List<DatabaseConnection> Connections { get; set; }
        public List<SqlQuery> Queries { get; set; }

        public DataAccessSettings()
        {
            Connections = new List<DatabaseConnection>();
            Queries = new List<SqlQuery>();
        }
    }
}