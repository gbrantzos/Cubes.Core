using System;
using Cubes.Core.Utilities;
using Microsoft.Extensions.Options;

namespace Cubes.Core.DataAccess
{
    public class QueryManager : IQueryManager
    {
        private readonly DataAccessOptions options;

        public QueryManager(IOptionsSnapshot<DataAccessOptions> options) =>
            this.options = options.Value;

        public Query GetSqlQuery(string queryName)
        {
            var queries = options.Queries;
            var result = queries.Find(i => i.Name.Equals(queryName, StringComparison.CurrentCultureIgnoreCase));
            if (result == null)
                throw new ArgumentException($"Could not find query with name {queryName.IfNullOrEmpty("(null OR Empty )")}");
            return result;
        }
    }
}