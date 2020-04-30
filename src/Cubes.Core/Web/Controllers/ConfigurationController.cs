using System;
using Autofac.Features.Indexed;
using Cubes.Core.Base;
using Cubes.Core.Configuration;
using Cubes.Core.Utilities;
using Cubes.Core.Web.UIHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Cubes.Core.Web.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class ConfigurationController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly ITypeResolver typeResolver;
        private readonly ISerializer serializer;
        private readonly IConfigurationWriter configurationWriter;

        public ConfigurationController(IConfiguration configuration,
            ITypeResolver typeResolver,
            IIndex<string, ISerializer> serializerDictionary,
            IConfigurationWriter configurationWriter)
        {
            this.configuration = configuration;
            this.typeResolver = typeResolver;
            this.serializer = serializerDictionary[CubesConstants.Serializer_JSON];
            this.configurationWriter = configurationWriter;
        }

        /// <summary>
        /// Get configuration
        /// </summary>
        /// <remarks>
        /// Gets strongly typed configuration object identified by <paramref name="configurationName"/>.
        /// </remarks>
        /// <param name="configurationName"></param>
        /// <returns></returns>
        [HttpGet("{configurationName}")]
        public IActionResult GetConfiguration(string configurationName)
        {
            var configurationType = typeResolver.GetByName(configurationName);
            if (configurationType == null)
                return BadRequest($"Could not resolve type '{configurationName}");

            var configurationInstance = configuration.GetSection(configurationType.Name).Get(configurationType) ??
                Activator.CreateInstance(configurationType);

            var vmConverterType = configurationType
                .GetAttribute<ViewModelConverterAttribute>()?
                .ViewModelConverterType;
            if (vmConverterType != null)
            {
                var vmConverter = Activator.CreateInstance(vmConverterType) as ViewModelConverter;
                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver()
                };
                return new JsonResult(vmConverter.ToViewModel(configurationInstance), jsonSerializerSettings);
            }
            return Ok(configurationInstance );
        }

        /// <summary>
        /// Save configuration
        /// </summary>
        /// <remarks>
        /// Saves configuration object from body (in JSON format), that corresponds to type named <paramref name="configurationName"/>.
        /// This method must be called using 'text/plain' Content-Type!
        /// </remarks>
        /// <param name="configurationName"></param>
        /// <param name="configurationJson"></param>
        /// <returns></returns>
        [Consumes("text/plain")]
        [HttpPost("{configurationName}")]
        public IActionResult SaveConfiguration(string configurationName, [FromBody] string configurationJson)
        {
            var configurationType = typeResolver.GetByName(configurationName);
            if (configurationType == null)
                return BadRequest($"Could not resolve type '{configurationName}");

            var configurationInstance = serializer.Deserialize(configurationJson, configurationType);
            configurationWriter.Save(configurationType, configurationInstance);

            return Ok($"Configuration {configurationType.Name} saved");
        }
    }
}
