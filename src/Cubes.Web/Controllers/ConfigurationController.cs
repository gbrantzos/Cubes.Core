using System.IO;
using System.Text;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using Cubes.Core.Base;
using Cubes.Core.Configuration;
using Cubes.Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Cubes.Web.Controllers
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

        [HttpGet("{configurationName}")]
        public IActionResult GetConfiguration(string configurationName)
        {
            var configurationType = typeResolver.GetByName(configurationName);
            if (configurationType == null)
                return BadRequest($"Could not resolve type '{configurationName}");

            var configurationInstance = configuration.GetSection(configurationType.Name).Get(configurationType);
            return Ok(configurationInstance);
        }

        [HttpPost("{configurationName}")]
        public async Task<IActionResult> SaveConfiguration(string configurationName)
        {
            string configurationJson;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
                 configurationJson = await reader.ReadToEndAsync();

            var configurationType = typeResolver.GetByName(configurationName);
            if (configurationType == null)
                return BadRequest($"Could not resolve type '{configurationName}");

            var configurationInstance = serializer.Deserialize(configurationJson, configurationType);
            configurationWriter.Save(configurationType, configurationInstance);

            return Ok();
        }

    }
}