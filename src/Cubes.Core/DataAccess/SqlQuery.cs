using System.Collections.Generic;

namespace Cubes.Core.DataAccess
{
    public class SqlQuery
    {
        public string Name { get; set; }
        public string Comments { get; set; }
        public string Query { get; set; }
        public List<SqlQueryParameter> Parameters { get; set; }
    }

    public class SqlQueryParameter
    {
        public string Name { get; set; }
        public string DbType { get; set; }
        public object Default { get; set; }
    }
}