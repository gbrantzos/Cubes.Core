using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace Cubes.Api.Settings
{
    [ApiController, Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        [HttpPost]
        public ActionResult Parse([FromBody]string raw)
        {
            var r = new StringReader(raw);
            var deserializer = new Deserializer().Deserialize(r);

            //var yamlObject = deserializer.Deserialize<dynamic>(r)["TestPaySignService"].Values;

            // just to print the json
            //var serializer = new JsonSerializer();
            //serializer.Serialize(Console.Out, yamlObject);

            return Ok(raw);
        }
    }
}
