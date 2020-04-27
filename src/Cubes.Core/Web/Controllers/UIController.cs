using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cubes.Core.Base;
using Cubes.Core.Commands;
using Cubes.Core.Utilities;
using Cubes.Core.Web.UIHelpers.Lookups;
using Cubes.Core.Web.UIHelpers.Schema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Cubes.Core.Web.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("ui")]
    public class UIController : ControllerBase
    {
        private static readonly HashSet<string> themes = new HashSet<string>
        {
            "feeling-blue",
            "flattop",
            "material",
            "monokai",
            "muted",
            "outline"
        };
        private static readonly string resourceRoot = "Cubes.Core.Web.Swagger.Themes";
        private readonly IConfiguration configuration;
        private readonly ISchemaManager schemaManager;
        private readonly List<ILookupProvider> lookupProviders;
        private readonly ITypeResolver typeResolver;

        public UIController(IConfiguration configuration,
            IEnumerable<ILookupProvider> lookupProviders,
            ITypeResolver typeResolver,
            ISchemaManager schemaManager)
        {
            this.configuration = configuration;
            this.schemaManager = schemaManager;
            this.lookupProviders = lookupProviders.ToList();
            this.typeResolver = typeResolver;
        }

        [HttpGet("swagger-css")]
        public IActionResult GetTheme()
        {
            var themeName = this.configuration.GetValue<string>(CubesConstants.Config_HostSwaggerTheme, "material");
            var css = string.Empty;
            if (themes.Contains(themeName))
            {
                var assembly = typeof(UIController).Assembly;
                using var stream = assembly.GetManifestResourceStream($"{resourceRoot}.theme-{themeName}.css");
                using var reader = new StreamReader(stream);
                css = reader.ReadToEnd().Trim();
            }
            return Content(css, "text/css");
        }

        [HttpGet("lookup/{lookupName}")]
        public IActionResult GetLookup(string lookupName)
        {
            var provider = this
                .lookupProviders
                .FirstOrDefault(pr => pr.Name.Equals(lookupName, StringComparison.CurrentCultureIgnoreCase));
            if (provider == null)
                return BadRequest($"Unknown lookup provider: {lookupName}");

            return Ok(provider.Get());
        }

        [HttpGet("schema/{schemaName}")]
        public IActionResult GetSchema(string schemaName)
        {
            return Ok(schemaManager.GetSchema(schemaName));
        }

        [HttpGet("requestSample/{providerName}")]
        public IActionResult GetSample(string providerName)
        {
            var type = typeResolver.GetByName(providerName);
            if (type == null)
                return BadRequest($"Could not create provider '{providerName}'!");

            var provider = Activator.CreateInstance(type) as IRequestSampleProvider;
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver()
            };

            return new JsonResult(provider.GetSample(), jsonSerializerSettings);
        }
    }
}
