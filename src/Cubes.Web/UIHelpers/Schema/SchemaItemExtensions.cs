using System;
using Cubes.Core.Utilities;
using Humanizer;

namespace Cubes.Web.UIHelpers.Schema
{
    public static class SchemaItemExtensions
    {
        private static void ApplyProperties(SchemaItem item, string key, string label, SchemaItemType type, params Validator[] validators)
        {
            key.ThrowIfEmpty(nameof(label));
            if (string.IsNullOrEmpty(label))
                label = key.Humanize(LetterCasing.Title);

            item.Type       = type;
            item.Key        = key;
            item.Label      = label;
            item.Validators = validators;
        }

        // Finish building and return to Schema!
        public static Schema Build(this SchemaItem item) => item.Schema;

        public static SchemaItem Text(this SchemaItem item, string key, string label, params Validator[] validators)
        {
            ApplyProperties(item, key, label, SchemaItemType.Text, validators);

            return item;
        }

        public static SchemaItem Text(this SchemaItem item, string key, params Validator[] validators)
            => item.Text(key, String.Empty, validators);

        public static SchemaItem TextArea(this SchemaItem item, string key, string label, int rows = 4, params Validator[] validators)
        {
            ApplyProperties(item, key, label, SchemaItemType.Textarea, validators);
            item.TextareaRows = rows;

            return item;
        }

        public static SchemaItem TextArea(this SchemaItem item, string key, int rows = 4, params Validator[] validators)
            => item.TextArea(key, String.Empty, rows, validators);

        public static SchemaItem Checkbox(this SchemaItem item, string key, string label, params Validator[] validators)
        {
            ApplyProperties(item, key, label, SchemaItemType.Checkbox, validators);

            return item;
        }

        public static SchemaItem Checkbox(this SchemaItem item, string key, params Validator[] validators)
            => item.Checkbox(key, String.Empty, validators);

        public static SchemaItem Datepicker(this SchemaItem item, string key, string label, params Validator[] validators)
        {
            ApplyProperties(item, key, label, SchemaItemType.Datepicker, validators);

            return item;
        }

        public static SchemaItem Datepicker(this SchemaItem item, string key, params Validator[] validators)
            => item.Datepicker(key, String.Empty, validators);

        public static SchemaItem Select(this SchemaItem item, string key, string label, Options options, params Validator[] validators)
        {
            ApplyProperties(item, key, label, SchemaItemType.Select, validators);
            item.Options = options;

            return item;
        }

        public static SchemaItem Select(this SchemaItem item, string key, Options options, params Validator[] validators)
            => item.Select(key, String.Empty, options, validators);

        public static SchemaItem HasClass(this SchemaItem item, string className)
        {
            item.ClassName = className;
            return item;
        }

        // TODO We can add methods like

        // StartsGroup
        // EndsGroup
        // FlexProperties
        // ...
    }
}
