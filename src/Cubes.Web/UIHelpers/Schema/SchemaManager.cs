using System.Collections.Generic;

namespace Cubes.Web.UIHelpers.Schema
{
    public class SchemaManager : ISchemaManager
    {
        public Schema GetSchema(string name)
        {
            var schema = new Schema
            {
                Name = "SmtpSettings",
                Label = "SMTP Settings"
            };

            schema.Items = new List<SchemaItem>
            {
                new SchemaItem
                {
                    Key = "name",
                    Label = "Name",
                    Type = SchemaItemType.Text,
                    Validators = new List<Validator>
                    {
                        new Validator {Name = ValidatorType.Required}
                    }
                },
                new SchemaItem
                {
                    Key = "host",
                    Label = "Host",
                    Type = SchemaItemType.Text,
                    Validators = new List<Validator>
                    {
                        new Validator {Name = ValidatorType.Required}
                    }
                },
                new SchemaItem
                {
                    Key = "port",
                    Label = "Port",
                    Type = SchemaItemType.Text,
                    Validators = new List<Validator>
                    {
                        new Validator {Name = ValidatorType.Required},
                        new Validator {Name = ValidatorType.Min, Parameters = 25}
                    }
                },
                new SchemaItem
                {
                    Key = "timeout",
                    Label = "Timeout",
                    Type = SchemaItemType.Text,
                    Validators = new List<Validator>
                    {
                        new Validator {Name = ValidatorType.Required}
                    }
                },
                new SchemaItem
                {
                    Key = "sender",
                    Label = "Sender",
                    Type = SchemaItemType.Text,
                    Validators = new List<Validator>
                    {
                        new Validator {Name = ValidatorType.Required}
                    }
                },
                new SchemaItem
                {
                    Key = "useSsl",
                    Label = "Use SSL",
                    Type = SchemaItemType.Checkbox
                },
                new SchemaItem
                {
                    Key = "userName",
                    Label = "User name",
                    Type = SchemaItemType.Text
                },
                new SchemaItem
                {
                    Key = "password",
                    Label = "Password",
                    Type = SchemaItemType.Text
                },
            };

            return schema;
        }
    }
}