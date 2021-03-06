using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Cubes.Core.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Cubes.Core.Web.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class SystemController : ControllerBase
    {
        private readonly ICubesEnvironment cubesEnvironment;
        private readonly IApiDescriptionGroupCollectionProvider apiExplorer;

        public SystemController(ICubesEnvironment cubesEnvironment,
            IApiDescriptionGroupCollectionProvider apiExplorer)
        {
            this.cubesEnvironment = cubesEnvironment;
            this.apiExplorer = apiExplorer;
        }

        /// <summary>
        /// Cubes server heartbeat
        /// </summary>
        /// <remarks>
        /// Return useful information about Cubes Server status.
        /// </remarks>
        [HttpGet("ping")]
        public ActionResult<string> Ping()
        {
            var envInfo = cubesEnvironment.GetEnvironmentInformation();
            var proc = Process.GetCurrentProcess();
            if (proc == null || proc.MainModule == null)
                throw new Exception("Could not get Cubes process information");
            var asm = Assembly.GetEntryAssembly();
            if (asm == null)
                throw new Exception("Could not get Cubes entry assembly information");

            var info = new
            {
                ProcID           = proc.Id,
                proc.ProcessName,
                Executable       = Path.GetFileName(proc.MainModule.FileName),
                Assembly         = asm.GetName().Name,
                WorkingFolder    = Path.GetDirectoryName(asm.Location),
                Machine          = proc.MachineName,
                envInfo.Hostname,
                CoreVersion      = envInfo.BuildVersion,
                Build            = envInfo.IsDebug ? "DEBUG" : "RELEASE",
                envInfo.BuildInformation,
                LiveSince        = envInfo.LiveSince.ToString("yyyy-MM-dd HH:mm:ss"),
                ServerTime       = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                WorkingSet       = Math.Round(proc.WorkingSet64 / 1024M / 1024M, 2),
                PeakWorkingSet   = Math.Round(proc.PeakWorkingSet64 / 1024M / 1024M, 2),
                Threads          = proc.Threads.Count,
                LoadedAssemblies = cubesEnvironment.GetLoadedAssemblies(),
                ConfiguredApps   = cubesEnvironment.GetLoadedeApplications(),
                ActivatedApps    = cubesEnvironment.GetApplicationInstances().Select(app => app.GetType().Name)
            };
            return Ok(info);
        }

        /// <summary>
        /// Cubes server routes
        /// </summary>
        /// <remarks>
        /// Lists all available routes with parameters details.
        /// </remarks>
        [HttpGet("routes")]
        public ActionResult Routes()
        {
            var routes = apiExplorer
               .ApiDescriptionGroups
               .Items
               .SelectMany(i => i.Items)
               .Select(i => new
               {
                   Method     = i.HttpMethod,
                   Path       = i.RelativePath,
                   Parameters = i.ParameterDescriptions.Select(p => new
                   {
                       p.Name,
                       Type   = p.ParameterDescriptor == null ?
                                    "Unknown" :
                                    $"{p.ParameterDescriptor.ParameterType.Name}, {p.ParameterDescriptor.ParameterType.FullName}",
                       Source = p.Source.ToString()
                   })
               });
            return Ok(routes);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("version")]
        public string Version()
        {
            var envInfo = cubesEnvironment.GetEnvironmentInformation();
            return envInfo.BuildVersion;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("file")]
        public IActionResult File()
        {
            return Content("css", "text/css");
        }
    }
}
