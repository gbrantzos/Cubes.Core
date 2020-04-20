using System;
using System.Threading.Tasks;
using Cubes.Core.Utilities;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Cubes.Core.Scheduling.Jobs
{
    // Sample Job for testing
    [DisallowConcurrentExecution]
    [Display("Sample Job")]
    public class SampleJob : IJob
    {
        private readonly ILogger<SampleJob> logger;

        public SampleJob(ILogger<SampleJob> logger) => this.logger = logger;

        public Task Execute(IJobExecutionContext context)
        {
            foreach (var item in context.JobDetail.JobDataMap)
                logger.LogDebug($"Key: {item.Key} - Value: {item.Value}");

            logger.LogInformation("Sample Job executed at {executionTime}...", DateTime.Now);

            // throw new ArgumentException("Failed!");
            context.Result = new { IsError = false, Message = "All fine!" };
            return Task.CompletedTask;
        }
    }
}
