using System.Collections.Generic;
using Cubes.Core.Base;

namespace Cubes.Core.DataAccess
{
    public class SampleQueryProvider : IQueryProvider
    {
        public string QueryName => "Core.SampleQuery";

        public Query GetQuery()
        {
            var type = GetType();
            return new Query
            {
                Name         = "SampleQuery",
                Comments     = "This is a sample query",
                QueryCommand = EmbeddedResourceManager.GetText(type.Assembly, type.Namespace, "Queries.SampleQuery.sql"),
                Parameters   = new List<QueryParameter>
                {
                    new QueryParameter{ Name ="p_prm", DbType = "String"}
                }
            };
        }
    }
}
