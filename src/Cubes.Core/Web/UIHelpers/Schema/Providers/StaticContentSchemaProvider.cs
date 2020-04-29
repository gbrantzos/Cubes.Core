using Cubes.Core.Web.StaticContent;

namespace Cubes.Core.Web.UIHelpers.Schema.Providers
{
    public class StaticContentSchemaProvider : SchemaProvider<Content>
    {
        public override Schema GetSchema()
            => Schema.Create(this.Name, "Static Content")
                .WithText("requestPath", Validator.Required(), Validator.Pattern("^[a-zA-Z0-9_\\-]+$"))
                .WithText("fileSystemPath", Validator.Required())
                .WithText("defaultFile", Validator.Required(), Validator.Pattern("^[a-zA-Z0-9_\\-\\.]+$"))
                .WithCheckbox("active")
                .WithCheckbox("serveUnknownFileTypes")
                .WithTextArea("customContentTypes");
    }
}
