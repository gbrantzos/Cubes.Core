using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cubes.Core.Base;
using Cubes.Core.Configuration;
using Cubes.Core.Scheduling;
using Microsoft.AspNetCore.Mvc;

namespace Cubes.Web.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class SchedulingController : ControllerBase
    {
        private readonly IScheduler scheduler;
        private readonly ICubesEnvironment cubesEnvironment;
        private readonly IConfigurationWriter configurationWriter;

        public SchedulingController(IScheduler scheduler,
            ICubesEnvironment cubesEnvironment,
            IConfigurationWriter configurationWriter)
        {
            this.scheduler           = scheduler;
            this.cubesEnvironment    = cubesEnvironment;
            this.configurationWriter = configurationWriter;
        }

        /// <summary>
        /// Scheduler status
        /// </summary>
        /// <remarks>Get current scheduler status with jobs details.</remarks>
        /// <returns><see cref="SchedulerStatus"/></returns>
        [HttpGet]
        public Task<SchedulerStatus> GetStatus() => scheduler.GetStatus();

        /// <summary>
        /// Execute Job
        /// </summary>
        /// <remarks>
        /// Execute Job defined in scheduler settings file, that is named with <paramref name="jobName"/>
        /// </remarks>
        /// <param name="jobName">Name of Job</param>
        /// <returns></returns>
        [HttpPost, Route("execute/{jobName}")]
        public ActionResult Execute(string jobName)
        {
            try
            {
                scheduler.ExecuteJob(jobName);
                return Ok("Job was triggered successfully!");
            }
            catch(ArgumentException x)
            {
                return BadRequest(x.Message);
            }
        }

        /// <summary>
        /// Send command to scheduler
        /// </summary>
        /// <remarks>
        /// Send command to scheduler. Valid commands are start, stop and reload.
        /// </remarks>
        /// <param name="command">Command name</param>
        /// <returns></returns>
        [HttpPost, Route("command/{command}")]
        public async Task<IActionResult> SendCommand(string command)
        {
            var knownCommands = new HashSet<string> { "start", "stop", "reload" };
            command = command.ToLower();
            if (!knownCommands.Contains(command))
                return BadRequest($"Command {command} is unknown!");

            if (command == "stop")
                await scheduler.Stop();
            if (command == "start")
                await scheduler.Start();
            if (command == "reload")
                await scheduler.Reload();

            return Ok(await scheduler.GetStatus());
        }

        /// <summary>
        /// Save scheduler jobs
        /// </summary>
        /// <param name="jobs">Array of <see cref="SchedulerJob"/></param>
        /// <remarks>
        /// Save scheduler jobs to the corresponding configuration file.
        /// </remarks>
        /// <returns></returns>
        [HttpPost, Route("save")]
        public IActionResult SaveJob([FromBody] SchedulerJob[] jobs)
        {
            var settings = new SchedulerSettings { Jobs = jobs };
            try
            { settings.Validate(); }
            catch (Exception x)
            {
                return BadRequest(x.ToString());
            }
            this.configurationWriter.Save(settings);

            return Ok("Scheduler settings saved!");
        }
    }
}
