using System.Collections.Generic;
using System.Linq;
using Cubes.Core.DataAccess;
using Cubes.Core.Utilities;

namespace Cubes.Core.Commands.Basic
{
    [Display("Execute SQL and wrap results in e-mail"), RequestSample(typeof(QueryResultsAsEmailSampleProvider))]
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
        {
            if (QuerySet.Queries.Count == 0)
                return "No queries defined, possibly a mistake!";

            return QuerySet.Queries.Count > 1 ?
                           $"Run {QuerySet.Queries.Count} queries on {QuerySet.ConnectionName} and send results as email" :
                           $"Run query {QuerySet.Queries.First().Name} on {QuerySet.ConnectionName} and send results as email";
        }
    }

    public class QueryResultsAsEmailSampleProvider : IRequestSampleProvider
    {
        public object GetSample()
        {
            return new QueryResultsAsEmail
            {
                QuerySet = new QuerySet
                {
                    ConnectionName = "A connection to use",
                    Queries = new List<QuerySetItem>
                    {
                        new QuerySetItem { Name = "Details page", QueryName = "Query.#1.Details" },
                        new QuerySetItem { Name = "Totals page", QueryName = "Query.#2.Totals" },
                    }
                },
                Subject = "E-mail subject",
                Body = "This is the body of the e-mail",
                ToAddresses = new List<string>
                {
                    "recipient1@somewhere.com",
                    "recipient2@somewhere.com"
                },
                SendIfDataExists = true,
                SmtpProfile = "SMTP.Profile#2",
                FileName = "Save.ToFile.xlsx"
            };
        }
    }
}
