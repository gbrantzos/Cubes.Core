using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cubes.Core.Scheduling;
using Microsoft.AspNetCore.Mvc;

namespace Cubes.Web.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class SchedulingController : ControllerBase
    {
        private readonly IScheduler scheduler;

        public SchedulingController(IScheduler scheduler) => this.scheduler = scheduler;

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
    }
}
