using System;
using System.Collections.Generic;
using System.Linq;
using Cubes.Core.DataAccess;
using Cubes.Core.Email;
using Cubes.Core.Utilities;

namespace Cubes.Core.Commands.Basic
{
    public class SqlResultsAsEmailHandler : BaseCommandHandler<SqlResultsAsEmailCommand, SqlResultsAsEmailResult>
    {
        private readonly IDatabaseConnectionManager connectionManager;
        private readonly IQueryExecutor queryExecutor;

        public SqlResultsAsEmailHandler(IDatabaseConnectionManager connectionManager, IQueryExecutor queryExecutor)
        {
            this.connectionManager = connectionManager;
            this.queryExecutor = queryExecutor;
        }

        protected override SqlResultsAsEmailResult HandleInternal(SqlResultsAsEmailCommand command)
        {
            if (command.SqlQueries.Count <= 0)
                throw new ArgumentException("No queries defined!");

            var results = GetQueryResults(command.DbConnection, command.SqlQueries);

            // Prepare email and attachments...
            var email = new EmailContent
            {
                Subject = command.Subject,
                ToAddresses = command.ToAddresses
            };
            bool rowsFound = results.Any(i => i.Value.Count() > 0);
            if (rowsFound)
                email.Attachments = new List<EmailContent.EmailAttachment>
                {
                    new EmailContent.EmailAttachment
                    {
                        FileName    = $"{command.FileName}_{ DateTime.Now.ToString("ddMMyyyy") }.xlsx",
                        ContentType = "application/vnd.ms-excel",
                        Content     = results.ToExcelPackage()
                    }
                };

            // Body details
            string suffix = results.Count() == 0 ? "y" : "ies";
            string resultInfo = rowsFound ?
                $"Executed {results.Count()} quer{suffix}, found {results.Select(i => i.Value.Count()).Sum()} rows." :
                "No rows found!";
            email.Body = String.IsNullOrEmpty(command.Body) ? String.Empty : command.Body + System.Environment.NewLine + System.Environment.NewLine;
            email.Body += resultInfo;

            // Return command result
            var toReturn = new SqlResultsAsEmailResult
            {
                Message      = resultInfo,
                SqlResults   = results,
                EmailContent = email,
                SmtpSettings = command.SmtpSettings
            };
            if (!rowsFound && command.SendIfDataExists)
                toReturn.EmailContent = null;
            return toReturn;
        }

        private Dictionary<string, IEnumerable<dynamic>> GetQueryResults(string connectionName, Dictionary<string, string> sqlQueries)
        {
            var toReturn = new Dictionary<string, IEnumerable<dynamic>>();

            foreach (var query in sqlQueries)
            {
                var results = queryExecutor.Query(connectionName, query.Value, null);
                toReturn.Add(query.Key, results);
            }

            return toReturn;
        }
    }
}
