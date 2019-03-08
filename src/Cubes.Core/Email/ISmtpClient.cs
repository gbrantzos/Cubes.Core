using System;
using System.Net;
using System.Net.Mail;

namespace Cubes.Core.Email
{
    public interface ISmtpClient : IDisposable
    {
        string Host { get; set; }
        int Port { get; set; }
        bool EnableSsl { get; set; }
        bool UseDefaultCredentials { get; set; }
        SmtpDeliveryMethod DeliveryMethod { get; set; }
        ICredentialsByHost Credentials { get; set; }

        void Send(MailMessage message);
    }
}