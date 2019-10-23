using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Cubes.Core.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Cubes.Web.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class SystemController : ControllerBase
    {
        private readonly ICubesEnvironment cubesEnvironment;
        private readonly IApiDescriptionGroupCollectionProvider apiExplorer;
        private readonly IContextProvider context;

        public SystemController(ICubesEnvironment cubesEnvironment,
            IApiDescriptionGroupCollectionProvider apiExplorer,
            IContextProvider context)
        {
            this.cubesEnvironment = cubesEnvironment;
            this.apiExplorer = apiExplorer;
            this.context = context;
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
            var info = new
            {
                ProcID           = proc.Id,
                ProcessName      = proc.ProcessName,
                Executable       = Path.GetFileName(proc.MainModule.FileName),
                Assembly         = Assembly.GetEntryAssembly().GetName().Name,
                WorkingFolder    = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                Machine          = proc.MachineName,
                Hostname         = envInfo.Hostname,
                KernelVersion    = envInfo.FileVersion,
                Build            = envInfo.IsDebug ? "DEBUG" : "RELEASE",
                LiveSince        = envInfo.LiveSince.ToString("yyyy-MM-dd HH:mm:ss"),
                ServerTime       = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                WorkingSet       = Math.Round(proc.WorkingSet64 / 1024M / 1024M, 2),
                PeakWorkingSet   = Math.Round(proc.PeakWorkingSet64 / 1024M / 1024M, 2),
                Threads          = proc.Threads.Count,
                LoadedAssemblies = cubesEnvironment.GetLoadedAssemblies(),
                LoadedApps       = cubesEnvironment.GetApplications()
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
                       Name   = p.Name,
                       Type   = p.ParameterDescriptor == null ?
                                    "Unknown" :
                                    $"{p.ParameterDescriptor.ParameterType.Name}, {p.ParameterDescriptor.ParameterType.FullName}",
                       Source = p.Source.ToString()
                   })
               });
            return Ok(routes);
        }
    }
}
