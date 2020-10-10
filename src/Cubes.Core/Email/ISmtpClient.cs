using System;
using MimeKit;

namespace Cubes.Core.Email
{
    public interface ISmtpClient
    {
        void Send(MimeMessage message, SmtpSettings smtpSettings);
    }
}