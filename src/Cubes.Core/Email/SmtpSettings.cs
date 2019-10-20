using System;
using System.Collections.Generic;
using System.Linq;

namespace Cubes.Core.Email
{
    public class SmtpSettingsProfiles
    {
        public ICollection<SmtpSettings> Profiles { get; set; } = new HashSet<SmtpSettings>();

        public SmtpSettings GetByName(string profileName)
        {
            var profile = Profiles
                .FirstOrDefault(p => p.Name.Equals(profileName, StringComparison.CurrentCultureIgnoreCase));

            return profile;
        }

        public static SmtpSettingsProfiles Create()
        {
            var toReturn = new SmtpSettingsProfiles();
            toReturn.Profiles.Add(new SmtpSettings());
            return toReturn;
        }
    }

    public class SmtpSettings
    {
        public string Name                 { get; set; }
        public string Host                 { get; set; }
        public int Port                    { get; set; }
        public int Timeout                 { get; set; }
        public string Sender               { get; set; }
        public bool UseSsl                 { get; set; }
        public SmtpCredentials Credentials { get; set; }
        public SmtpSettings()
        {
            Name    = "Default";
            Host    = "localhost";
            Port    = 25;
            Timeout = 600;
            Sender  = "no-reply@somewhere.com";

            Credentials = new SmtpCredentials
            {
                UserName = "user",
                Password = "password"
            };
        }

        public class SmtpCredentials
        {
            public string UserName { get; set; }
            public string Password { get; set; }
        }
    }

}