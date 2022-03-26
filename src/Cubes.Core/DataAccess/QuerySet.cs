using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;

namespace Cubes.Core.DataAccess
{
    public class QuerySet
    {
        public string ConnectionName { get; set; }
        public ICollection<QuerySetItem> Queries { get; set; } = new HashSet<QuerySetItem>();

        public static IEnumerable<QueryResult> GetQueryResults(QuerySet querySet,
            IConnectionManager connectionManager,
            IQueryManager queryManager)
        {
            var toReturn = new List<QueryResult>();
            foreach (var queryItem in querySet.Queries)
            {
                using var cnx = connectionManager.GetConnection(querySet.ConnectionName);
                var query = queryManager.GetSqlQuery(queryItem.QueryName);
                var results = cnx.Query(query.QueryCommand).ToList();

                var queryResult = new QueryResult
                {
                    Name = queryItem.Name,
                    Data = results,
                    Metadata = query.Metadata
                };
                var firstRow = (IDictionary<string, object>)results.FirstOrDefault();
                if (firstRow != null)
                {
                    var headers = firstRow.Keys.ToArray();
                    var values = firstRow.Values.Select(v => v?.GetType() ?? typeof(object)).ToArray();
                    queryResult.Columns = headers
                        .Zip(values, (h, v) => new QueryResult.Column
                        {
                            Name = h,
                            ColumnType = v
                        })
                        .ToList();
                }

                toReturn.Add(queryResult);
            }

            return toReturn;
        }
    }

    public class QuerySetItem
    {
        public string Name { get; set; }
        public string QueryName { get; set; }
    }

    public class QueryResult
    {
        public string Name { get; set; }
        public IEnumerable<dynamic> Data { get; set; } = new List<dynamic>();
        public IEnumerable<Column> Columns { get; set; } = new List<Column>();

        public class Column
        {
            public string Name { get; set; }
            public Type ColumnType { get; set; }
        }

        public QueryMetadata Metadata { get; set; }
    }
}