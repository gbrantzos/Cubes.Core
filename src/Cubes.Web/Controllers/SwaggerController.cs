using System.Collections.Generic;
using System.IO;
using Cubes.Core.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Cubes.Web.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("swagger")]
    public class SwaggerController : ControllerBase
    {
        private static HashSet<string> themes = new HashSet<string>
        {
            "feeling-blue",
            "flattop",
            "material",
            "monokai",
            "muted",
            "outline"
        };
        private static string resourceRoot = "Cubes.Web.Swagger.Themes";
        private readonly IConfiguration configuration;

        public SwaggerController(IConfiguration configuration)
            => this.configuration = configuration;

        [HttpGet("css")]
        public ActionResult GetTheme()
        {
            var themeName = this.configuration.GetValue<string>(CubesConstants.Config_HostSwaggerTheme, "material");
            var css = string.Empty;
            if (themes.Contains(themeName))
            {
                var assembly = typeof(SwaggerController).Assembly;
                using var stream = assembly.GetManifestResourceStream($"{resourceRoot}.theme-{themeName}.css");
                using var reader = new StreamReader(stream);
                css = reader.ReadToEnd().Trim();
            }
            return Content(css, "text/css");
        }
    }
}
