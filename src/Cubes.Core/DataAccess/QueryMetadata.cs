using System.Collections.Generic;

namespace Cubes.Core.DataAccess
{
    public class QueryMetadata
    {
        public int FixedColumns { get; set; }
        public string TotalsLabel { get; set; } = "Totals";
        public List<QueryMetadataColumn> Columns { get; set; } = new List<QueryMetadataColumn>();
    }

    public class QueryMetadataColumn
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public string Format { get; set; }
        public bool HasTotals { get; set; }
    }
}