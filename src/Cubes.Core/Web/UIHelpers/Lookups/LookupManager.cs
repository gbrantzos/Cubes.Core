using System;
using System.Collections.Generic;
using System.Linq;

namespace Cubes.Core.Web.UIHelpers.Lookups
{
    public class LookupManager : ILookupManager
    {
        private readonly IEnumerable<ILookupProvider> providers;

        public LookupManager(IEnumerable<ILookupProvider> providers)
        {
            this.providers = providers;
        }

        public Lookup GetLookup(string name)
        {
            // Search for appropriate provider
            var provider = providers
                .FirstOrDefault(pr => pr.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            if (provider == null)
                throw new ArgumentException($"No lookup provider registered for {name}");

            // Get lookup
            return provider.GetLookup();
        }
    }
}