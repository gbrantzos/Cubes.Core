using System;
using System.Linq;
using Cubes.Core.Utilities;
using Humanizer;

namespace Cubes.Web.UIHelpers.Schema
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
                Key = key,
                Label = label,
                Type = type,
                Validators = validators
            };
        }

        private static void AddItem(this Schema schema, SchemaItem item)
        {
            var existing = schema.Items.FirstOrDefault(i => i.Key == item.Key);
            if (existing == null)
                schema.Items.Add(item);
            else
                existing = item;
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
            var item = CreateItem(key, label, SchemaItemType.Checkbox, validators);
            item.Options = options;
            schema.AddItem(item);

            return schema;
        }

        public static Schema WithSelect(this Schema schema, string key, Options options, params Validator[] validators)
            => schema.WithSelect(key, String.Empty, options, validators);
    }
}