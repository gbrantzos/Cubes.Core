using System.Collections.Generic;

namespace Cubes.Core.DataAccess
{
    public class QuerySet
    {
        public string Name { get; set; }
        public string ConnectionName { get; set; }

        public ICollection<QuerySetItem> Queries { get; set; } = new HashSet<QuerySetItem>();
    }

    public class QuerySetItem
    {
        public string Name { get; set; }
        public string QueryName { get; set; }
    }
}
