using System;
using System.Linq;
using Cubes.Core.Utilities;
using Humanizer;

namespace Cubes.Core.Web.UIHelpers.Schema
{
    public static class SchemaExtensions
    {
        private static SchemaItem CreateItem(string key, string label, SchemaItemType type, params Validator[] validators)
        {
            key.ThrowIfEmpty(nameof(label));
            if (string.IsNullOrEmpty(label))
                label = key.Humanize(LetterCasing.Title);

            return new SchemaItem
            {
                Key        = key,
                Label      = label,
                Type       = type,
                Validators = validators
            };
        }

        public static Schema WithItem(this Schema schema, SchemaItem item)
        {
            item.Key.ThrowIfEmpty(nameof(item.Key));
            if (string.IsNullOrEmpty(item.Label))
                item.Label = item.Key.Humanize(LetterCasing.Title);

            schema.AddItem(item);
            return schema;
        }

        public static Schema WithText(this Schema schema, string key, string label, params Validator[] validators)
        {
            var item = CreateItem(key, label, SchemaItemType.Text, validators);
            schema.AddItem(item);

            return schema;
        }

        public static Schema WithText(this Schema schema, string key, params Validator[] validators)
            => schema.WithText(key, String.Empty, validators);

        public static Schema WithCheckbox(this Schema schema, string key, string label, params Validator[] validators)
        {
            var item = CreateItem(key, label, SchemaItemType.Checkbox, validators);
            schema.AddItem(item);

            return schema;
        }

        public static Schema WithCheckbox(this Schema schema, string key, params Validator[] validators)
            => schema.WithCheckbox(key, String.Empty, validators);

        public static Schema WithTextArea(this Schema schema, string key, string label, int rows = 4, params Validator[] validators)
        {
            var item = CreateItem(key, label, SchemaItemType.Textarea, validators);
            item.TextareaRows = rows;
            schema.AddItem(item);

            return schema;
        }

        public static Schema WithTextArea(this Schema schema, string key, int rows = 4, params Validator[] validators)
            => schema.WithTextArea(key, String.Empty, rows, validators);

        public static Schema WithTextArea(this Schema schema, string key, params Validator[] validators)
            => schema.WithTextArea(key, String.Empty, 4, validators);

        public static Schema WithDatepicker(this Schema schema, string key, string label, params Validator[] validators)
        {
            var item = CreateItem(key, label, SchemaItemType.Datepicker, validators);
            schema.AddItem(item);

            return schema;
        }

        public static Schema WithDatepicker(this Schema schema, string key, params Validator[] validators)
            => schema.WithDatepicker(key, String.Empty, validators);

        public static Schema WithSelect(this Schema schema, string key, string label, Options options, params Validator[] validators)
        {
            var item = CreateItem(key, label, SchemaItemType.Select, validators);
            item.Options = options;
            schema.AddItem(item);

            return schema;
        }

        public static Schema WithSelect(this Schema schema, string key, Options options, params Validator[] validators)
            => schema.WithSelect(key, String.Empty, options, validators);

        public static Schema WithSelectDynamic(this Schema schema, string key, string label, string lookupKey, params Validator[] validators)
        {
            var item = CreateItem(key, label, SchemaItemType.Select, validators);
            item.Options = new Options { Dynamic = true, LookupKey = lookupKey };
            schema.AddItem(item);

            return schema;
        }

        public static Schema WithSelectDynamic(this Schema schema, string key, string lookupKey, params Validator[] validators)
            => schema.WithSelectDynamic(key, String.Empty, lookupKey, validators);

        public static Schema WithPassword(this Schema schema, string key, string label, params Validator[] validators)
        {
            var item = CreateItem(key, label, SchemaItemType.Password, validators);
            schema.AddItem(item);

            return schema;
        }

        public static Schema WithPassword(this Schema schema, string key, params Validator[] validators)
            => schema.WithPassword(key, String.Empty, validators);

        public static Schema SetItemHint(this Schema schema, string hint)
        {
            schema.Items.Last().Hint = hint;
            return schema;
        }

        public static Schema SetItemFlex(this Schema schema, string flex)
        {
            schema.Items.Last().Flex = flex;
            return schema;
        }
    }
}
