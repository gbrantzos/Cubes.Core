using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cubes.Web.UIHelpers.Schema
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Schema
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public ICollection<SchemaItem> Items { get; set; } = new List<SchemaItem>();

        // Factory methods
        public static Schema Create(string name, string label) => new Schema { Name = name, Label = label};
        public static Schema Create(string name) => Create(name, name);
    }
}