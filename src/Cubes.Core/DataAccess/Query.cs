using System.Collections.Generic;

namespace Cubes.Core.DataAccess
{
    public class Query
    {
        public string Name { get; set; }
        public string Comments { get; set; }
        public string QueryCommand { get; set; }
        public List<QueryParameter> Parameters { get; set; }
    }

    public class QueryParameter
    {
        public string Name { get; set; }
        public string DbType { get; set; } = "String";
        public object Default { get; set; }
    }
}