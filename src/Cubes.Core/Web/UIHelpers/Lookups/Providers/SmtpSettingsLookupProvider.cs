using System.Linq;
using Cubes.Core.Email;
using Microsoft.Extensions.Options;

namespace Cubes.Core.Web.UIHelpers.Lookups.Providers
{
    public class SmtpSettingsLookupProvider : ILookupProvider
    {
        private readonly SmtpSettingsProfiles _smtpSettings;

        public string Name => LookupProviders.SmptSettings;

        public SmtpSettingsLookupProvider(IOptionsSnapshot<SmtpSettingsProfiles> optionsSnapshot)
        {
            _smtpSettings = optionsSnapshot.Value;
        }

        public Lookup GetLookup()
        {
            return new Lookup
            {
                Name = LookupProviders.SmptSettings,
                Cacheable = false,
                Items = _smtpSettings
                    .Profiles
                    .Select(prf => new LookupItem
                    {
                        Value = prf.Name,
                        Display = prf.Comments
                    })
                    .OrderBy(i => i.Display)
                    .ToList()
            };
        }
    }
}
