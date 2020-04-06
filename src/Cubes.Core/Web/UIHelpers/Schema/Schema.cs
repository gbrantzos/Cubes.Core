using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Cubes.Core.Web.UIHelpers.Schema
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Schema
    {
        public string Name { get; set; }
        public string Label { get; set; }

        private List<SchemaItem> items = new List<SchemaItem>();
        public IEnumerable<SchemaItem> Items => items.AsEnumerable();

        // Helper for Fluent API
        public SchemaItem WithItem
        {
            get
            {
                var item = new SchemaItem { Schema = this };
                this.items.Add(item);

                return item;
            }
        }

        // Factory methods
        public static Schema Create(string name, string label) => new Schema { Name = name, Label = label};
        public static Schema Create(string name) => Create(name, name);

        public void AddItem(SchemaItem item)
        {
            var existing = Items.FirstOrDefault(i => i.Key == item.Key);
            if (existing == null)
                items.Add(item);
            else
                existing = item;
            item.Schema = this;
        }
    }
}