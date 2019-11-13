using Cubes.Core.Email;

namespace Cubes.Web.UIHelpers.Schema.CoreProviders
{
    public class SmtpSettingsProfilesSchemaProvider : SchemaProvider<SmtpSettingsProfiles>
    {
        public override Schema GetSchema()
        {
            var schema = Schema
                .Create(this.Name, "SMTP Profiles")
                .AddText("name", "Name", Validator.Required())
                .AddText("host", "Host", Validator.Required())
                .AddText("port", "Port", Validator.Required(), Validator.Min(25))
                .AddText("timeout", "Timeout", Validator.Required())
                .AddText("sender", "Sender", Validator.Required())
                .AddCheckbox("useSsl", "Use SSL")
                .AddText("userName", "User Name")
                .AddText("password", "Password");

            return schema;
        }
    }
}