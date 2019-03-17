using System;
using Cubes.Core.Settings;
using Cubes.Core.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace Cubes.Api.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly ITypeResolver typeResolver;
        private readonly ISettingsProvider settingsProvider;

        public SettingsController(ITypeResolver typeResolver, ISettingsProvider settingsProvider)
        {
            this.typeResolver = typeResolver;
            this.settingsProvider = settingsProvider;
        }

        /// <summary>
        /// Get settings using <see cref="ISettingsProvider"/>.
        /// </summary>
        /// <remarks>
        /// Provides access to settings 
        /// </remarks>
        /// <param name="settingsType">Settings type</param>
        /// <param name="profile">Settings profile. Default profile returned if profile equals "default".</param>
        /// <returns></returns>
        [HttpGet, Route("{settingsType}/{profile=default}")]
        public ActionResult Get(string settingsType, string profile = "default")
        {
            var type = typeResolver.GetByName(settingsType);
            if (type == null)
                return BadRequest($"Could not resolve setting type: {settingsType}");

            var settings = settingsProvider.Load(type, profile.Equals("default", StringComparison.CurrentCultureIgnoreCase) ? String.Empty : profile);
            return Ok(settings);
        }
    }
}
