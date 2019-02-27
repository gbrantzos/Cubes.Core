using Cubes.Core.Settings;
using System;
using System.Linq;

namespace Cubes.Core.DataAccess
{
    public class SqlQueryManager : ISqlQueryManager
    {
        private readonly ISettingsProvider settingsProvider;

        public SqlQueryManager(ISettingsProvider settingsProvider) =>
            this.settingsProvider = settingsProvider;

        public SqlQuery GetSqlQuery(string queryName)
        {
            var queries = settingsProvider.Load<DataAccessSettings>().Queries;
            var result = queries.FirstOrDefault(i => i.Name.Equals(queryName, StringComparison.CurrentCultureIgnoreCase));
            if (result == null)
                throw new ArgumentException($"Could not find query: {queryName}");
            return result;
        }
    }
}