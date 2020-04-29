using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cubes.Core.DataAccess
{
    public interface IDefaultQueries
    {
        Query GetQuery(string name);
        IEnumerable<string> GetQueryNames();
    }

    public class DefaultQueries : IDefaultQueries
    {
        private readonly IEnumerable<IQueryProvider> queryProviders;

        public DefaultQueries()
        {
            this.queryProviders = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(asm => asm.GetTypes())
                .Where(type => type.GetInterfaces().Contains(typeof(IQueryProvider)))
                .Select(t => Activator.CreateInstance(t) as IQueryProvider)
                .ToList();
        }

        public Query GetQuery(string name)
        {
            var provider = queryProviders
                .FirstOrDefault(p => p.QueryName.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (provider == null)
                throw new ArgumentException($"Could not find provider for query '{name}'.");

            return provider.GetQuery();
        }

        public IEnumerable<string> GetQueryNames() => queryProviders.Select(p => p.QueryName).ToList();
    }
}
