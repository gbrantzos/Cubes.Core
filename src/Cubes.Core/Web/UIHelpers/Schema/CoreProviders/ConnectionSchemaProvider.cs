using System.Collections.Generic;
using System.Linq;
using Cubes.Core.DataAccess;
using Cubes.Core.Web.UIHelpers.Lookups;

namespace Cubes.Core.Web.UIHelpers.Schema.CoreProviders
{
    public class ConnectionSchemaProvider : SchemaProvider<Connection>
    {
        private readonly ILookupProvider lookupProvider;

        public ConnectionSchemaProvider(IEnumerable<ILookupProvider> lookupProviders)
            => this.lookupProvider = lookupProviders.First(pv => pv.Name == "DbProviders");

        public override Schema GetSchema()
            => Schema.Create(this.Name, "Database Connection")
                .WithText("name", Validator.Required(), Validator.Pattern(@"^\S+$"))
                .WithText("comments")
                .WithSelect("dbProvider", "Database Provider", lookupProvider.Get().ToOptions(), Validator.Required())
                .WithTextArea("connectionString", 3, Validator.Required());
    }
}
