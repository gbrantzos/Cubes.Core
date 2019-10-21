using System.Collections.Generic;
using System.Linq;
using Cubes.Core.DataAccess;
using Cubes.Core.Utilities;

namespace Cubes.Core.Commands.Basic
{
    [Display("Execute SQL and wrap results in e-mail")]
    public class QueryResultsAsEmail : Request<QueryResultsAsEmailResult>
    {
        public QuerySet QuerySet { get; set; } = new QuerySet();
        public string SmtpProfile { get; set; }
        public string Subject { get; set; }
        public List<string> ToAddresses { get; set; }
        public string Body { get; set; }
        public bool SendIfDataExists { get; set; }
        public string FileName { get; set; }

        public override string ToString()
            => QuerySet.Queries.Count > 1 ?
                $"Run {QuerySet.Queries.Count} queries on {QuerySet.ConnectionName} and send results as email" :
                $"Run query {QuerySet.Queries.First().Name} on {QuerySet.ConnectionName} and send results as email";
    }
}
