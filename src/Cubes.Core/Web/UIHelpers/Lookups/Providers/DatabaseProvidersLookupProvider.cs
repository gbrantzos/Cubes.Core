using System.Collections.Generic;
using System.Linq;
using Cubes.Core.DataAccess;

namespace Cubes.Core.Web.UIHelpers.Lookups.Providers
{
    public class DatabaseProvidersLookupProvider : ILookupProvider
    {
        private static readonly Dictionary<string, string> knownProviders = new Dictionary<string, string>
        {
            { "Oracle.ManagedDataAccess", "Oracle" },
            { "Microsoft.Data.SqlClient", "SQL Server" },
            { "MySql.Data"              , "mySQL" },
            { "Npgsql"                  , "PostgreSQL"}
        };

        public string Name => LookupProviders.DatabaseProviders;

        public Lookup GetLookup()
        {
            var registeredProviders = ConnectionManager.RegisteredProviders;
            return new Lookup
            {
                Name      = this.Name,
                Cacheable = true,
                Items     = registeredProviders
                    .Select(pv => new LookupItem
                    {
                        Value   = pv.Value,
                        Display = knownProviders.ContainsKey(pv.Key) ? knownProviders[pv.Key] : pv.Key
                    })
                    .OrderBy(i => i.Display)
                    .ToList()
            };
        }
    }
}
