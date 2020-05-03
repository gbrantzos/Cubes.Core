using Cubes.Core.Web.UIHelpers.Lookups;
using Cubes.Core.Web.UIHelpers.Schema;

namespace Cubes.Core.Base.Samples
{
    public static class SampleApplicationOptionsSchema
    {
        public static ComplexSchema GetSchema()
        {
            var cs = new ComplexSchema
            {
                Name = "SampleApplication"
            };
            cs.Sections.Add(new ComplexSchemaSection
            {
                RootProperty = "Basic",
                Schema = Schema.Create("Basic")
                    .WithSelectDynamic("SEnConnection", "SEn Connection", LookupProviders.DataConnections)
                    .WithSelectDynamic("OdwConnection", "ODW Connection", LookupProviders.DataConnections)
                    .WithCheckbox("CheckEofExistence", "Check EOF label existence")
                    .WithTextArea("CheckEofExistenceExceptions", "Codes to skip EOF labels existence check")
                // Comma separated list of codes to exclude from EOF label check
            });
            cs.Sections.Add(new ComplexSchemaSection
            {
                RootProperty = "WmsUsers",
                Schema = Schema.Create("Users", "WMS Users")
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
