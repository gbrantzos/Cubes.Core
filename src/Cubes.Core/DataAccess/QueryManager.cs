using System;
using System.Linq;
using Cubes.Core.Utilities;
using Microsoft.Extensions.Options;

namespace Cubes.Core.DataAccess
{
    public class QueryManager : IQueryManager
    {
        private readonly DataAccessSettings settings;

        public QueryManager(IOptionsSnapshot<DataAccessSettings> options) =>
            this.settings = options.Value;

        public Query GetSqlQuery(string queryName)
        {
            var queries = settings.Queries;
            var result = queries.FirstOrDefault(i => i.Name.Equals(queryName, StringComparison.CurrentCultureIgnoreCase));
            if (result == null)
                throw new ArgumentException($"Could not find query with name {queryName.IfNullOrEmpty("(null OR Empty )")}");
            return result;
        }
    }
}