using System.Threading;
using System.Threading.Tasks;
using Cubes.Core.Utilities;
using Quartz;

namespace Cubes.Core.Jobs
{
    public class JobExecutionListener : IJobListener
    {
        private readonly IJobExecutionHistory executionHistory;

        public string Name => "CubesNext :: Job Execution Listener";
        public JobExecutionListener(IJobExecutionHistory executionHistory)
            => this.executionHistory = executionHistory.ThrowIfNull(nameof(executionHistory));

        public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default(CancellationToken))
            => Task.CompletedTask;
        public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default(CancellationToken))
            => Task.CompletedTask;

        public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = default(CancellationToken))
        {
            var jobExecution = new JobExecution
            {
                JobID      = context.JobDetail.Key.Name,
                ExecutedAt = context.ScheduledFireTimeUtc.Value.DateTime,
                Duration   = context.JobRunTime,
                ExecutionResult = new JobExecutionResult()
            };
            if (context.JobDetail.JobDataMap.ContainsKey(QuartzJobDataParameters.MESSAGE_KEY))
                jobExecution.ExecutionResult.Message = context.JobDetail.JobDataMap.GetString(QuartzJobDataParameters.MESSAGE_KEY);

            executionHistory.Add(jobExecution);
            return Task.CompletedTask;
        }
    }
}