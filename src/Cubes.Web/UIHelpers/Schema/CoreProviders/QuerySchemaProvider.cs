using Cubes.Core.DataAccess;

namespace Cubes.Web.UIHelpers.Schema.CoreProviders
{
    public class QuerySchemaProvider : SchemaProvider<Query>
    {
        public override Schema GetSchema()
        {
            return Schema.Create(this.Name, "Database Query")
               .WithText("name", Validator.Required())
               .WithText("comments")
               .WithTextArea("queryCommand", 8, Validator.Required());
        }
    }
}
