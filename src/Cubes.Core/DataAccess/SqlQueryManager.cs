using System;
using System.Linq;
using Microsoft.Extensions.Options;

namespace Cubes.Core.DataAccess
{
    public class SqlQueryManager : ISqlQueryManager
    {
        private readonly DataAccessSettings settings;

        public SqlQueryManager(IOptionsSnapshot<DataAccessSettings> options) =>
            this.settings = options.Value;

        public SqlQuery GetSqlQuery(string queryName)
        {
            var queries = settings.Queries;
            var result = queries.FirstOrDefault(i => i.Name.Equals(queryName, StringComparison.CurrentCultureIgnoreCase));
            if (result == null)
                throw new ArgumentException($"Could not find query: {queryName}");
            return result;
        }
    }
}