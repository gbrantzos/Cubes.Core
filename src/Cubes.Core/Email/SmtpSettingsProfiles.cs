using System;
using System.Collections.Generic;
using System.Linq;
using Cubes.Core.Base;
using Cubes.Core.Configuration;

namespace Cubes.Core.Email
{
    [ConfigurationStore(CubesConstants.Files_SmtpSettings)]
    public class SmtpSettingsProfiles
    {
        public ICollection<SmtpSettings> Profiles { get; set; } = new HashSet<SmtpSettings>();

        /// <summary>
        /// Get <see cref="SmtpSettings"/> profile with given name, or null if no profile found
        /// </summary>
        /// <param name="profileName">Profile name</param>
        /// <returns></returns>
        public SmtpSettings GetByName(string profileName)
            => Profiles
                .FirstOrDefault(p => p.Name.Equals(profileName, StringComparison.CurrentCultureIgnoreCase));

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
        public string Comments             { get; set; }
        public string Host                 { get; set; }
        public int Port                    { get; set; }
        public int Timeout                 { get; set; }
        public string Sender               { get; set; }
        public bool UseSsl                 { get; set; }
        public SmtpCredentials Credentials { get; set; }
        public SmtpSettings()
        {
            Name     = "Default";
            Comments = "Default SMTP profile";
            Host     = "localhost";
            Port     = 25;
            Timeout  = 600;
            Sender   = "no-reply@somewhere.com";

            /*Credentials = new SmtpCredentials
            {
                UserName = "user",
                Password = "password"
            };*/
        }

        public class SmtpCredentials
        {
            public string UserName { get; set; }
            public string Password { get; set; }
        }
    }
}