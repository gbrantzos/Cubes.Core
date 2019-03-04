using System;
using System.Net;
using System.Net.Mail;

namespace Cubes.Core.Email
{
    public class SmtpClientWrapper : ISmtpClient, IDisposable
    {
        private SmtpClient client;
        public SmtpClientWrapper() => client = new SmtpClient();

        public string Host { get => client.Host; set => client.Host = value; }
        public int Port { get => client.Port; set => client.Port = value; }
        public bool EnableSsl { get => client.EnableSsl; set => client.EnableSsl = value; }
        public bool UseDefaultCredentials { get => client.UseDefaultCredentials; set => client.UseDefaultCredentials = value; }
        public SmtpDeliveryMethod DeliveryMethod { get => client.DeliveryMethod; set => client.DeliveryMethod = value; }
        public ICredentialsByHost Credentials { get => client.Credentials; set => client.Credentials = value; }

        public void Dispose() => client?.Dispose();

        public void Send(MailMessage message) => client.Send(message);
    }
}