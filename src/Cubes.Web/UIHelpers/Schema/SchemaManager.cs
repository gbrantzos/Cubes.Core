using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Cubes.Web.UIHelpers.Schema
{
    public class SchemaManager : ISchemaManager
    {
        private ConcurrentDictionary<string, Schema> cache = new ConcurrentDictionary<string, Schema>();
        private readonly List<ISchemaProvider> providers;

        public SchemaManager(IEnumerable<ISchemaProvider> providers)
        {
            this.providers = providers.ToList();
        }

        public Schema GetSchema(string name)
        {
            // Search on cache
            if (cache.TryGetValue(name, out var schema))
                return schema;

            // Search for appropriate provider
            var provider = providers.FirstOrDefault(pr => pr.Name == name);
            if (provider == null)
                throw new ArgumentException($"No schema provider registered for {name}");

            // Get schema and add on cache
            schema = provider.GetSchema();
            cache.AddOrUpdate(name, schema, (n, s) => schema);

            return schema;
        }
    }
}