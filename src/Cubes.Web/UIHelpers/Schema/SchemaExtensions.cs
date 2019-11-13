namespace Cubes.Web.UIHelpers.Schema
{
    public static class SchemaExtensions
    {
        private static SchemaItem CreateItem(string key, string label, SchemaItemType type, params Validator[] validators)
        {
            var item = new SchemaItem
            {
                Key = key,
                Label = label,
                Type = type,
                Validators = validators
            };
            return item;
        }
        public static Schema AddText(this Schema schema, string key, string label, params Validator[] validators)
        {
            var item = CreateItem(key, label, SchemaItemType.Text, validators);
            schema.Items.Add(item);

            return schema;
        }

        public static Schema AddCheckbox(this Schema schema, string key, string label, params Validator[] validators)
        {
            var item = CreateItem(key, label, SchemaItemType.Checkbox, validators);
            schema.Items.Add(item);

            return schema;
        }

        // TODO Add missing methods
        // TODO Add methods without label, support label guessing!
    }
}