using Cubes.Core.Jobs;
using Microsoft.AspNetCore.Mvc;

namespace Cubes.Api.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class SchedulerController : ControllerBase
    {
        private readonly IJobScheduler jobScheduler;

        public SchedulerController(IJobScheduler jobScheduler)
        {
            this.jobScheduler = jobScheduler;
        }

        [HttpGet]
        public ActionResult<SchedulerStatus> GetStatus()
            => jobScheduler.GetStatus();

        [HttpPost, Route("{command}")]
        public ActionResult<SchedulerStatus> SendCommand(string command)
        {

            return jobScheduler.GetStatus();
        }
    }
}
