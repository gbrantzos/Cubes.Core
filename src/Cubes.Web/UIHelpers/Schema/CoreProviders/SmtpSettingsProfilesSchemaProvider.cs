using Cubes.Core.Email;

namespace Cubes.Web.UIHelpers.Schema.CoreProviders
{
    public class SmtpSettingsProfilesSchemaProvider : SchemaProvider<SmtpSettingsProfiles>
    {
        public override Schema GetSchema()
            => Schema.Create(this.Name, "SMTP Profiles")
                .WithText("name", Validator.Required())
                .WithText("host", Validator.Required())
                .WithText("port", Validator.Required(), Validator.Min(25))
                .WithText("timeout", Validator.Required())
                .WithText("sender", Validator.Required())
                .WithCheckbox("useSsl", "Use SSL")
                .WithText("userName")
                .WithText("password");
    }
}