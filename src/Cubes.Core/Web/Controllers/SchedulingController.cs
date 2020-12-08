using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cubes.Core.Base;
using Cubes.Core.Configuration;
using Cubes.Core.Scheduling;
using Cubes.Core.Scheduling.ExecutionHistory;
using Microsoft.AspNetCore.Mvc;

namespace Cubes.Core.Web.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class SchedulingController : ControllerBase
    {
        private readonly IScheduler _scheduler;
        private readonly IConfigurationWriter _configurationWriter;
        private readonly IExecutionHistoryManager _historyManager;

        public SchedulingController(IScheduler scheduler,
            IConfigurationWriter configurationWriter,
            IExecutionHistoryManager historyManager)
        {
            _scheduler           = scheduler;
            _configurationWriter = configurationWriter;
            _historyManager      = historyManager;
        }

        /// <summary>
        /// Scheduler status
        /// </summary>
        /// <remarks>Get current scheduler status with jobs details.</remarks>
        /// <returns><see cref="SchedulerStatus"/></returns>
        [HttpGet]
        public Task<SchedulerStatus> GetStatus() => _scheduler.GetStatus();

        /// <summary>
        /// Execute Job
        /// </summary>
        /// <remarks>
        /// Execute Job defined in scheduler settings file, that is named with <paramref name="jobName"/>
        /// </remarks>
        /// <param name="jobName">Name of Job</param>
        /// <returns></returns>
        [HttpPost, Route("execute/{jobName}")]
        public async Task<ActionResult> Execute(string jobName)
        {
            try
            {
                await _scheduler.ExecuteJob(jobName);
                return Ok("Job was triggered successfully!");
            }
            catch (ArgumentException x)
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
                await _scheduler.Stop();
            if (command == "start")
                await _scheduler.Start();
            if (command == "reload")
                await _scheduler.Reload();

            return Ok(await _scheduler.GetStatus());
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
        public async Task<IActionResult> SaveJob([FromBody] SchedulerJob[] jobs)
        {
            var settings = new SchedulerSettings { Jobs = jobs };
            try
            { settings.Validate(); }
            catch (Exception x)
            {
                return BadRequest(x.ToString());
            }
            this._configurationWriter.Save(settings);

            // Give some time to IConfiguration to grab changes!
            await Task.Delay(1500);
            await _scheduler.Reload();

            return Ok(await _scheduler.GetStatus());
        }

        /// <summary>
        /// Get scheduler job execution history.
        /// </summary>
        /// <remarks>
        /// Get execution history details for requested job.
        /// </remarks>
        /// <param name="jobName">Job name</param>
        /// <returns>Returns execution history as an <see cref="IEnumerable{T}"/> of <see cref="ExecutionHistoryDetails"/>.</returns>
        [HttpGet, Route("{jobName}/history")]
        public IEnumerable<ExecutionHistoryDetails> JobHistory(string jobName) => _historyManager.Get(jobName);
    }
}
