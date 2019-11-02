using System.Collections.Generic;

namespace Cubes.Web.UIHelpers.Lookups
{
    public class Lookup
    {
        public string Name { get; set; }
        public ICollection<LookupItem> Items { get; set; }
    }

    public class LookupItem
    {
        public string Value { get; set; }
        public string Display { get; set; }
        public string Group { get; set; }
        public object OtherData { get; set; }
    }
}
