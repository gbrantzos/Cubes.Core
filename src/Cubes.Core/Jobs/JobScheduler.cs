using System;
using System.Collections.Generic;
using System.Linq;
using Cubes.Core.Settings;
using Cubes.Core.Utilities;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl.Matchers;

namespace Cubes.Core.Jobs
{
    public class JobScheduler : IJobScheduler, IDisposable
    {
        private readonly ISettingsProvider settingsProvider;
        private readonly List<JobDefinition> loadedJobs;
        private readonly IScheduler quartzScheduler;
        private readonly IJobExecutionHistory jobExecutionHistory;
        private readonly ILogger<JobScheduler> logger;
        private readonly ISerializer serializer;

        public JobScheduler(ISettingsProvider settingsProvider,
            IScheduler quartzScheduler,
            IJobExecutionHistory jobExecutionHistory,
            ILogger<JobScheduler> logger,
            ISerializer serializer)
        {
            this.settingsProvider    = settingsProvider;
            this.quartzScheduler     = quartzScheduler;
            this.jobExecutionHistory = jobExecutionHistory;
            this.logger              = logger;
            this.serializer          = serializer;
            this.loadedJobs          = new List<JobDefinition>();
        }

        public SchedulerStatus GetStatus()
        {
            var jobKeys = quartzScheduler
                .GetJobKeys(GroupMatcher<Quartz.JobKey>.AnyGroup())
                .Result
                .Select(i => quartzScheduler.GetJobDetail(i).Result.Key.Name)
                .ToList();
            var schedulerStatus = new SchedulerStatus
            {
                State = quartzScheduler.InStandbyMode ? SchedulerState.Stopped : SchedulerState.Started,
                ServerTime = DateTime.Now,
                Jobs = loadedJobs
                    .Where(i => jobKeys.Contains(i.ID)) // Make sure defintion is loaded
                    .Select(i => new JobStatus
                    {
                        Definition      = i,
                        LastExecution   = jobExecutionHistory.GetLast(i.ID),
                        NextExecutionAt = quartzScheduler.GetTriggersOfJob(new JobKey(i.ID, "Cubes"))
                                .Result
                                .FirstOrDefault()?
                                .GetNextFireTimeUtc()?
                                .ToLocalTime()
                                .DateTime
                    })
                    .ToList()
            };
            return schedulerStatus;
        }

        public void LoadJobs()
        {
            var jobs = settingsProvider.Load<JobSchedulerSettings>()?.Jobs;
            if (jobs == null)
                throw new ArgumentNullException("JobScheduler settings must define jobs!");

            if (jobs.Count == 0)
                return;

            loadedJobs.Clear();
            foreach (var job in jobs)
                loadedJobs.Add(job);
        }

        public SchedulerStatus StartScheduler()
        {
            LoadJobs();
            var activeJobs = loadedJobs.Where(i => i.IsActive).ToList();
            if (activeJobs.Count == 0)
            {
                logger.LogWarning("No active jobs defined, Job Scheduler has nothing to do!");
            }
            else
            {
                quartzScheduler.Clear();
                foreach (var job in activeJobs)
                {
                    var trigger = TriggerBuilder
                        .Create()
                        .WithCronSchedule(job.CronExpression, i =>
                            {
                                if (job.FireIfMissed)
                                    i.WithMisfireHandlingInstructionFireAndProceed();
                                else
                                    i.WithMisfireHandlingInstructionDoNothing();
                            })
                        .StartNow();
                    var jobBuilder = JobBuilder
                        .Create<ExecuteCommandJob>()
                        .WithIdentity(job.ID.ToString(), "Cubes")
                        .WithDescription(job.Description);
                    if (job.ExecutionParameters != null)
                        jobBuilder.UsingJobData(QuartzJobDataParameters.PARAMETERS_KEY, serializer.Serialize(job.ExecutionParameters));

                    var scheduledJob = jobBuilder.Build();
                    quartzScheduler.ScheduleJob(scheduledJob, trigger.Build());
                    if (!job.IsActive)
                        quartzScheduler.PauseJob(scheduledJob.Key);
                }
                logger.LogInformation($"Starting job scheduler with {activeJobs.Count} active jobs.");
                quartzScheduler.Start();
            }
            return GetStatus();
        }

        public SchedulerStatus StopScheduler()
        {
            if (quartzScheduler.IsStarted)
            {
                logger.LogInformation("Stoping Job Scheduler.");
                quartzScheduler.Standby();
            }
            return GetStatus();
        }

        public void ExecuteJob(string jobID)
        {
            var job = this.loadedJobs.FirstOrDefault(i => i.ID.Equals(jobID));
            if (job == null)
                throw new ArgumentException($"Job with ID {jobID} not found!");

            var jobData = new JobDataMap();
            this.quartzScheduler.TriggerJob(new JobKey(jobID, "Cubes"));
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (quartzScheduler != null)
                        quartzScheduler.Shutdown();
                }
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}