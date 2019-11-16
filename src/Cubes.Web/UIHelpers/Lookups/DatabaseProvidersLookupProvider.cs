using System.Collections.Generic;
using System.Linq;
using Cubes.Core.DataAccess;

namespace Cubes.Web.UIHelpers.Lookups
{
    public class DatabaseProvidersLookupProvider : ILookupProvider
    {
        private static Dictionary<string, string> knownProviderNames = new Dictionary<string, string>
        {
            { "oracle", "Oracle" },
            { "mssql",  "SQL Server" },
            { "mysql",  "mySQL" },
        };

        public string Name => "DbProviders";

        public Lookup Get()
        {
            var knownProviders = ConnectionManager.KnownProviders;
            return new Lookup
            {
                Name  = this.Name,
                Items = knownProviders
                    .Select(pv => new LookupItem
                    {
                        Value   = pv.Key,
                        Display = knownProviderNames[pv.Key]
                    })
                    .OrderBy(i => i.Display)
                    .ToList()
            };
        }
    }
}
