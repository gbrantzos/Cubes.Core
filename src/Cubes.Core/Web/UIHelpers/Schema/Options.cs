using System.Collections.Generic;

namespace Cubes.Core.Web.UIHelpers.Schema
{
    public class Options
    {
        public bool Dynamic { get; set; }
        public string LookupKey { get; set; }
        public bool MultipleOptions { get; set; }
        public ICollection<OptionsItem> Items { get; set; } = new List<OptionsItem>();
    }

    public class OptionsItem
    {
        public string Label { get; set; }
        public string Value { get; set; }
        public bool Disabled { get; set; }
        public object OtherData { get; set; }
    }
}