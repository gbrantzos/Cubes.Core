using System.Collections.Generic;
using Dapper;

namespace Cubes.Core.DataAccess
{
    public class QuerySet
    {
        public string Name { get; set; }
        public string ConnectionName { get; set; }
        public ICollection<QuerySetItem> Queries { get; set; } = new HashSet<QuerySetItem>();

        public static Dictionary<string, IEnumerable<dynamic>> GetQueryResults(QuerySet querySet,
            IConnectionManager connectionManager,
            IQueryManager queryManager)
        {
            var toReturn = new Dictionary<string, IEnumerable<dynamic>>();

            foreach (var queryItem in querySet.Queries)
            {
                using (var cnx = connectionManager.GetConnection(querySet.ConnectionName))
                {
                    var query = queryManager.GetSqlQuery(queryItem.QueryName);
                    var results = cnx.Query<dynamic>(query.QueryCommand);
                    toReturn.Add(queryItem.Name, results);
                }
            }

            return toReturn;
        }
    }

    public class QuerySetItem
    {
        public string Name { get; set; }
        public string QueryName { get; set; }
    }
}
