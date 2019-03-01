using Cubes.Core.Settings;

namespace Cubes.Core.Email
{

    [SettingsPrefix("Core")]
    public class SmtpSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public int Timeout { get; set; }
        public string Sender { get; set; }
        public SmtpCredentials Credentials { get; set; }
        public bool UseSsl { get; set; }
        public SmtpSettings()
        {
            Host = "localhost";
            Port = 25;
            Timeout = 600;
            Sender = "no-reply@somewhere.com";
        }

        public class SmtpCredentials
        {
            public string UserName { get; set; }
            public string Password { get; set; }
        }
    }

}