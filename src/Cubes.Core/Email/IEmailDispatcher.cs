namespace Cubes.Core.Email
{
    public interface IEmailDispatcher
    {
        void DispatchEmail(EmailContent content, SmtpSettings smtpSettings);
    }
}