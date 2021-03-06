using System.Collections.Generic;
using System.Linq;
using Cubes.Core.Web.UIHelpers.Schema;

namespace Cubes.Core.Web.UIHelpers.Lookups
{
    public class Lookup
    {
        public string Name { get; set; }
        public ICollection<LookupItem> Items { get; set; }
        public bool Cacheable { get; set; }
    }

    public class LookupItem
    {
        public string Value { get; set; }
        public string Display { get; set; }
        public string Group { get; set; }
        public object OtherData { get; set; }
    }

    public static class LookupExtensions
    {
        public static Options ToOptions(this Lookup lookup, bool isMulti = false)
            => new Options
            {
                MultipleOptions = isMulti,
                Items = lookup
                    .Items
                    .Select(i => new OptionsItem
                    {
                        Value = i.Value,
                        Label = i.Display,
                        OtherData = i.OtherData
                    }).ToList()
            };
    }
}
