using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cubes.Core.Web.UIHelpers.Schema
{
    public enum SchemaItemType
    {
        Text,
        Textarea,
        Select,
        Checkbox,
        Datepicker,
        Password
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class SchemaItem
    {
        public string Key { get; set; }
        public string Label { get; set; }

        [JsonConverter(typeof(CustomStringEnumConverter))]
        public SchemaItemType Type { get; set; }

        public int? TextareaRows { get; set; }
        public int? TextareaMaxRows { get; set; }
        public Options Options { get; set; }
        public ICollection<Validator> Validators { get; set; } = new List<Validator>();
        public string ClassName { get; set; }
        public string Hint { get; set; }
        public string Flex { get; set; }

        [JsonIgnore]
        public Schema Schema { get; set; }

        public bool ShouldSerializeValidators() => Validators?.Count > 0;
        public bool ShouldSerializeClassName() => !String.IsNullOrEmpty(ClassName);
    }
}