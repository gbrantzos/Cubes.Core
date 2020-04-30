using System.Collections.Generic;

namespace Cubes.Core.Web.UIHelpers.Schema
{
    public class ComplexSchema
    {
        public string Name { get; set; }
        public IList<ComplexSchemaSection> Sections { get; set; }

        public ComplexSchema()
            => Sections = new List<ComplexSchemaSection>();
    }

    public class ComplexSchemaSection
    {
        public Schema Schema { get; set; }
        public bool IsList { get; set; }

        public bool SimpleList { get; set; }
        public string ListItem { get; set; }
        public string ListItemSub { get; set; }
        public string ListIcon { get; set; }
    }
}
