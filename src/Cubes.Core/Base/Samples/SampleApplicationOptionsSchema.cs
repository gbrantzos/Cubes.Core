using Cubes.Core.Web.UIHelpers.Lookups;
using Cubes.Core.Web.UIHelpers.Schema;

namespace Cubes.Core.Base.Samples
{
    public static class SampleApplicationOptionsSchema
    {
        public static ComplexSchema GetSchema()
        {
            var cs = new ComplexSchema { Name = "SampleApplication" };
            cs.Sections.Add(new ComplexSchemaSection
            {
                RootProperty = "Basic",
                Schema = Schema.Create("Basic")
                    .WithSelectDynamic("ConnectionString", "Connection String", LookupProviders.DataConnections)
                    .WithSelectDynamic("Endpoint", "Endpoint", LookupProviders.DataConnections)
                    .WithCheckbox("CheckExistence", "Check existence")
                    .WithTextArea("CheckExistenceExceptions", "Codes to skip existence check")
                        .SetItemHint("Comma separated list of codes to exclude from check")
            });
            cs.Sections.Add(new ComplexSchemaSection
            {
                RootProperty = "Users",
                Schema       = Schema.Create("Users", "Users")
                    .WithText("DisplayName", Validator.Required())
                    .WithText("UserName", Validator.Required())
                    .WithPassword("Password", Validator.Required()),
                IsList = true,
                ListDefinition = new ListDefinition
                {
                    Item        = "DisplayName",
                    ItemSub     = "UserName",
                    ItemSubExpr = "return `Username: ${listItem.UserName}`;",
                    Icon        = "far fa-user"
                }
            });

            return cs;
        }
    }
}
