using System.Collections.Generic;

namespace Cubes.Web.UIHelpers.Schema
{
    public class Options
    {
        public bool MultipleOptions { get; set; }
        public ICollection<OptionsItem> Items { get; set; } = new List<OptionsItem>();
    }

    public class OptionsItem
    {
        public string Label { get; set; }
        public string Value { get; set; }
        public bool Disabled { get; set; }
    }
}