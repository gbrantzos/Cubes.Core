using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Cubes.Core.Email
{
    public class EmailDispatcher : IEmailDispatcher
    {

        public virtual void DispatchEmail(EmailContent content, SmtpSettings smtpSettings)
        {
            using (var mail = new MailMessage())
            using (var client = new SmtpClient())
            {
                var cleanUp = new List<Tuple<MemoryStream, StreamWriter>>();

                client.Port = smtpSettings.Port;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Host = smtpSettings.Host;
                client.EnableSsl = smtpSettings.UseSsl;
                mail.Subject = content.Subject;
                mail.Body = content.Body;
                if (content.Attachments?.Count() > 0)
                {
                    foreach (var item in content.Attachments)
                    {
                        var stream = new MemoryStream(item.Content);
                        var writer = new StreamWriter(stream);
                        var attachment = new Attachment(stream, item.FileName, item.ContentType);
                        mail.Attachments.Add(attachment);

                        cleanUp.Add(new Tuple<MemoryStream, StreamWriter>(stream, writer));
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
                    i.Item1.Dispose();
                    i.Item2.Dispose();
                });
            }
        }
    }
}