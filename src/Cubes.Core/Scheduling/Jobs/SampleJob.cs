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

            //throw new JobExecutionException("Execution failed!", new ArgumentException("Failed to do something interesting!"));
            const string message = "Sample job finished successfully!";
            context.Result = new { IsError = false, Message = message };
            context.Put(Scheduler.MessageKey, message);
            return Task.CompletedTask;
        }
    }
}
