using System.Collections.Generic;
using Cubes.Core.Base;
using Cubes.Core.Configuration;

namespace Cubes.Core.DataAccess
{
    [ConfigurationStore(CubesConstants.Files_DataAccess)]
    public class DataAccessSettings
    {
        public List<Connection> Connections { get; set; }
        public List<Query> Queries { get; set; }

        public DataAccessSettings()
        {
            Connections = new List<Connection>();
            Queries = new List<Query>();
        }

        public static DataAccessSettings Create()
        {
            var toReturn = new DataAccessSettings();
            toReturn.Connections.Add(new Connection
            {
                Name             = "SampleConnection",
                DbProvider       = "mssql",
                ConnectionString = "Server=(local);Database=myDataBase;Trusted_Connection=True;"
            });
            toReturn.Queries.Add(new Query
            {
                Name         = "SampleQuery",
                Comments     = "This is a sample query",
                QueryCommand = "select * from ...",
                Parameters   = new List<QueryParameter>
                {
                    new QueryParameter{ Name = "@prm", Default = "1" }
                }
            });
            return toReturn;
        }
    }
}