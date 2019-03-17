using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Cubes.Core.Email
{
    public class EmailDispatcher : IEmailDispatcher, IDisposable
    {
        private ISmtpClient client;

        public EmailDispatcher(ISmtpClient client) => this.client = client;

        public virtual void DispatchEmail(EmailContent content, SmtpSettings smtpSettings)
        {
            using (var mail = new MailMessage())
            {
                var cleanUp = new List<(MemoryStream Stream, StreamWriter Writer)>();

                client.Port                  = smtpSettings.Port;
                client.DeliveryMethod        = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Host                  = smtpSettings.Host;
                client.EnableSsl             = smtpSettings.UseSsl;
                mail.Subject                 = content.Subject;
                mail.Body                    = content.Body;
                if (content.Attachments?.Count() > 0)
                {
                    foreach (var item in content.Attachments)
                    {
                        var stream = new MemoryStream(item.Content);
                        var writer = new StreamWriter(stream);
                        var attachment = new Attachment(stream, item.FileName, item.ContentType);
                        mail.Attachments.Add(attachment);

                        cleanUp.Add((stream, writer));
                    }
                }
                mail.From = new MailAddress(smtpSettings.Sender);
                mail.To.Add(String.Join(",", content.ToAddresses));
                mail.BodyEncoding = Encoding.UTF8;
                if (smtpSettings.Credentials != null)
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(smtpSettings.Credentials.UserName, smtpSettings.Credentials.Password);
                }
                client.Send(mail);
                cleanUp.ForEach(i =>
                {
                    i.Stream.Dispose();
                    i.Writer.Dispose();
                });
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    client?.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose() => Dispose(true);
        #endregion
    }
}