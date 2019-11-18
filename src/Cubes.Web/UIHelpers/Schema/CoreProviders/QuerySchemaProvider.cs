using Cubes.Core.DataAccess;

namespace Cubes.Web.UIHelpers.Schema.CoreProviders
{
    public class QuerySchemaProvider : SchemaProvider<Query>
    {
        public override Schema GetSchema()
        {
            var queryCommand = new SchemaItem
            {
                Key          = "queryCommand",
                Type         = SchemaItemType.Textarea,
                TextareaRows = 8,
                ClassName    = "code",
                Validators   = new[] { Validator.Required() }
            };

            return Schema.Create(this.Name, "Database Query")
               .WithText("name", Validator.Required())
               .WithText("comments")
               .WithItem(queryCommand);
        }
    }
}
