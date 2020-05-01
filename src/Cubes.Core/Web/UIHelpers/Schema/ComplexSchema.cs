using System.Collections.Generic;

namespace Cubes.Core.Web.UIHelpers.Schema
{
    public class ComplexSchema
    {
        /// <summary>
        /// Name of complex schema.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Sections of complex schema.
        /// </summary>
        public IList<ComplexSchemaSection> Sections { get; set; }

        public ComplexSchema()
            => Sections = new List<ComplexSchemaSection>();
    }

    public class ComplexSchemaSection
    {
        /// <summary>
        /// Used to link view model property with schema object.
        /// </summary>
        public string RootProperty { get; set; }

        /// <summary>
        /// Schema for section or popup editor.
        /// </summary>
        public Schema Schema       { get; set; }

        /// <summary>
        /// If true, section is a list.
        /// </summary>
        public bool IsList         { get; set; }

        // List specification
        public string ListItem     { get; set; }
        public string ListItemSub  { get; set; }
        public string ListIcon     { get; set; }
    }
}
