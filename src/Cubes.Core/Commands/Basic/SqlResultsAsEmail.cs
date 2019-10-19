using System.Collections.Generic;
using System.Linq;
using Cubes.Core.Email;
using Cubes.Core.Utilities;

namespace Cubes.Core.Commands.Basic
{
    [Display("Execute SQL and wrap results in e-mail")]
    public class SqlResultsAsEmail : Request<SqlResultsAsEmailResult>
    {
        public string DbConnection { get; set; }
        public Dictionary<string, string> SqlQueries { get; set; }
        public SmtpSettings SmtpSettings { get; set; }
        public string Subject { get; set; }
        public List<string> ToAddresses { get; set; }
        public string Body { get; set; }
        public bool SendIfDataExists { get; set; }
        public string FileName { get; set; }

        public override string ToString()
        {
            var text = SqlQueries.Count > 1 ?
                $"Run {SqlQueries.Count} queries on {DbConnection} and send as email" :
                $"Run query {SqlQueries.First().Value} on {DbConnection} and send as email";
            return text;
        }
    }
}
