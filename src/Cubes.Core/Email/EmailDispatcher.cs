using System.Collections.Generic;
using System.IO;
using System.Linq;
using MimeKit;

namespace Cubes.Core.Email
{
    public class EmailDispatcher : IEmailDispatcher
    {
        private readonly ISmtpClient _client;

        public EmailDispatcher(ISmtpClient client) => _client = client;

        public virtual void DispatchEmail(EmailContent content, SmtpSettings smtpSettings)
        {
            var cleanup = new List<MemoryStream>();
            var mail = new MimeMessage();
            mail.Subject = content.Subject;
            mail.To.AddRange(content.ToAddresses.Select(adr => new MailboxAddress(adr)));
            mail.From.Add(new MailboxAddress(smtpSettings.Sender));

            var bodyBuilder = new BodyBuilder { TextBody = content.Body };
            if (content.Attachments?.Count() > 0)
            {
                foreach (var item in content.Attachments)
                {
                    var ms = new MemoryStream(item.Content);
                    bodyBuilder.Attachments.Add(item.FileName, ms);
                    cleanup.Add(ms);
                }
            }
            mail.Body = bodyBuilder.ToMessageBody();
            _client.Send(mail, smtpSettings);

            cleanup.ForEach(m => m?.Dispose());
        }
    }
}