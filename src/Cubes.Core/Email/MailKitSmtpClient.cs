using System.IO;
using System.Security.Authentication;
using Cubes.Core.Base;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Cubes.Core.Email
{
    public class MailKitSmtpClient : ISmtpClient
    {
        private readonly ICubesEnvironment _environment;

        public MailKitSmtpClient(ICubesEnvironment environment)
        {
            _environment = environment;
        }

        public void Send(MimeMessage message, SmtpSettings smtpSettings)
        {
            var path = Path.Combine(_environment.GetFolder(CubesFolderKind.Logs), "smtp.log");
            using var client = new SmtpClient(new ProtocolLogger(path, false));
            client.SslProtocols = SslProtocols.Tls12;
            client.Connect(smtpSettings.Host,
                smtpSettings.Port,
                smtpSettings.UseSsl ? SecureSocketOptions.Auto : SecureSocketOptions.None);
            if (smtpSettings.Credentials != null)
            {
                if (client.AuthenticationMechanisms.Contains("NTLM"))
                {
                    var ntlm = new SaslMechanismNtlm(
                        smtpSettings.Credentials.UserName,
                        smtpSettings.Credentials.Password);
                    client.Authenticate(ntlm);
                }
                else
                {
                    // use the default supported mechanisms
                    client.Authenticate(smtpSettings.Credentials.UserName, smtpSettings.Credentials.Password);
                }
            }

            client.Timeout = 60 * 1000; // 1 minute! This should be configurable
            client.Send(message);
            client.Disconnect(true);
        }
    }
}
