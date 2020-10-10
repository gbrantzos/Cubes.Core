using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Cubes.Core.Email
{
    public class MailKitSmtpClient : ISmtpClient
    {
        public void Send(MimeMessage message, SmtpSettings smtpSettings)
        {
            using SmtpClient client = new SmtpClient();
            client.Connect(smtpSettings.Host,
                smtpSettings.Port,
                smtpSettings.UseSsl ? SecureSocketOptions.Auto : SecureSocketOptions.None);
            if (smtpSettings.Credentials != null)
                client.Authenticate(smtpSettings.Credentials.UserName, smtpSettings.Credentials.Password);

            client.Send(message);
            client.Disconnect(true);
        }
    }
}
