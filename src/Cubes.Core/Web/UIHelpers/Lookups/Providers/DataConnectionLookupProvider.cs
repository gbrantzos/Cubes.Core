using System;
using System.Linq;
using Cubes.Core.DataAccess;
using Microsoft.Extensions.Options;

namespace Cubes.Core.Web.UIHelpers.Lookups.Providers
{
    public class DataConnectionLookupProvider : ILookupProvider
    {
        public string Name => LookupProviders.DataConnections;

        public DataAccessSettings settings { get; }

        public DataConnectionLookupProvider(IOptionsSnapshot<DataAccessSettings> optionsSnapshot)
        {
            this.settings = optionsSnapshot.Value;
        }

        public Lookup GetLookup()
        {
            return new Lookup
            {
                Name = this.Name,
                Cacheable = false,
                Items = this.settings
                    .Connections
                    .Select(c => new LookupItem
                    {
                        Value = c.Name,
                        Display = String.IsNullOrEmpty(c.Comments) ? c.Name : c.Comments
                    })
                    .OrderBy(l => l.Display)
                    .ToList()
            };
        }
    }
}
