using Cubes.Core.Settings;
using System.Collections.Generic;

namespace Cubes.Core.DataAccess
{
    [SettingsPrefix("Core")]
    public class DataAccessSettings
    {
        public List<Connection> Connections { get; set; }
        public List<Query> Queries { get; set; }

        public DataAccessSettings()
        {
            Connections = new List<Connection>();
            Queries = new List<Query>();
        }
    }
}