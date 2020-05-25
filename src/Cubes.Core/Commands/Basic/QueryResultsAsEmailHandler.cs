using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cubes.Core.DataAccess;
using Cubes.Core.Email;
using Cubes.Core.Utilities;
using Microsoft.Extensions.Options;

namespace Cubes.Core.Commands.Basic
{
    public class QueryResultsAsEmailHandler : RequestHandler<QueryResultsAsEmail, QueryResultsAsEmailResult>
    {
        private readonly IConnectionManager connectionManager;
        private readonly IQueryManager queryManager;
        private readonly SmtpSettingsProfiles smtpProfiles;

        public QueryResultsAsEmailHandler(IConnectionManager connectionManager,
            IQueryManager queryManager,
            IOptionsSnapshot<SmtpSettingsProfiles> options)
        {
            this.connectionManager = connectionManager;
            this.queryManager = queryManager;
            this.smtpProfiles = options.Value;
        }

        protected override Task<QueryResultsAsEmailResult> HandleInternal(QueryResultsAsEmail command, CancellationToken cancellationToken)
        {
            if (command.QuerySet.Queries.Count == 0)
                throw new ArgumentException("No queries defined!");

            var results = QuerySet.GetQueryResults(command.QuerySet, connectionManager, queryManager);

            // Prepare email and attachments...
            var email = new EmailContent
            {
                Subject = command.Subject,
                ToAddresses = command.ToAddresses
            };
            bool rowsFound = results.Any(i => i.Data.Any());
            if (rowsFound)
            {
                email.Attachments = new List<EmailContent.EmailAttachment>
                {
                    new EmailContent.EmailAttachment
                    {
                        FileName    = $"{command.FileName}_{ DateTime.Now:ddMMyyyy}.xlsx",
                        ContentType = "application/vnd.ms-excel",
                        Content     = results.ToExcelPackage()
                    }
                };
            }

            // Body details
            string suffix = results.Count() == 1 ? "y" : "ies";
            string resultInfo = rowsFound ?
                $"Executed {results.Count()} quer{suffix}, found {results.Select(i => i.Data.Count()).Sum()} rows." :
                "No rows found!";
            email.Body = String.IsNullOrEmpty(command.Body) ?
                String.Empty :
                command.Body + System.Environment.NewLine + System.Environment.NewLine;
            email.Body += resultInfo;

            // Return command result
            MessageToReturn = resultInfo;
            var toReturn = new QueryResultsAsEmailResult
            {
                SqlResults   = results.ToDictionary(d => d.Name, d => d.Data),
                EmailContent = email,
                SmtpSettings = this.smtpProfiles.GetByName(command.SmtpProfile)
            };
            if (!rowsFound && command.SendIfDataExists)
                toReturn.EmailContent = null;
            return Task.FromResult(toReturn);
        }
    }
}
