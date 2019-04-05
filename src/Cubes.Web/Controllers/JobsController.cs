using System;
using System.Collections.Generic;
using System.Linq;
using Cubes.Core.Jobs;
using Microsoft.AspNetCore.Mvc;

namespace Cubes.Web.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly IJobScheduler jobScheduler;
        private readonly IJobExecutionHistory executionHistory;

        public JobsController(IJobScheduler jobScheduler, IJobExecutionHistory executionHistory)
        {
            this.jobScheduler = jobScheduler;
            this.executionHistory = executionHistory;
        }

        [HttpGet]
        public ActionResult<SchedulerStatus> GetStatus()
            => jobScheduler.GetStatus();

        [HttpPost, Route("{jobID}")]
        public ActionResult Execute(string jobID)
        {
            try
            {
                jobScheduler.ExecuteJob(jobID);
                return Ok("Job was triggered successfully!");
            }
            catch(ArgumentException x)
            {
                return BadRequest(x.Message);
            }
        }

        [HttpPost, Route("{command}")]
        public ActionResult<SchedulerStatus> SendCommand(string command)
        {
            var knownCommands = new HashSet<string> { "start", "stop", "reload" };
            command = command.ToLower();
            if (!knownCommands.Contains(command))
                return BadRequest($"Command {command} is unknown!");

            if (command == "stop")
                jobScheduler.StopScheduler();
            if (command == "start")
                jobScheduler.StartScheduler();
            if (command == "reload")
            {
                jobScheduler.StopScheduler();
                jobScheduler.StartScheduler();
            }

            return jobScheduler.GetStatus();
        }

        [HttpGet, Route("{jobID}/history/{maxItems=5}")]
        public ActionResult<IEnumerable<JobExecution>> GetHistory(string jobID, int maxItems = 5)
            => executionHistory
                .GetAll(jobID)
                .OrderByDescending(i => i.ExecutedAt)
                .Take(maxItems)
                .ToList();

        [HttpPost, Route("{jobID}/clearhistory")]
        public ActionResult ClearHistory(string jobID, [FromBody]HistoryRetentionOptions options)
        {
            var deleted = executionHistory.ClearHistory(jobID, options);
            return Ok($"Deleted {deleted} entries!");
        }
    }
}
