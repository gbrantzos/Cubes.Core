using System;
using System.Collections.Generic;
using System.Linq;
using Cubes.Core.Settings;
using Cubes.Core.Utilities;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Cubes.Core.Jobs
{
    public class JobScheduler : IJobScheduler, IDisposable
    {
        public const string PARAMETERS_KEY = "ExecutionParameters";


        private readonly ISettingsProvider settingsProvider;
        private readonly List<(JobDefinition definition, JobExecution execution)> jobDetails;
        private readonly IScheduler quartzScheduler;
        private readonly ILogger<JobScheduler> logger;
        private readonly ITypeResolver typeResolver;

        public JobScheduler(ISettingsProvider settingsProvider, IScheduler quartzScheduler, ILogger<JobScheduler> logger, ITypeResolver typeResolver)
        {
            this.settingsProvider = settingsProvider;
            this.quartzScheduler = quartzScheduler;
            this.logger = logger;
            this.typeResolver = typeResolver;
            this.jobDetails = new List<(JobDefinition definition, JobExecution execution)>();
            this.LoadJobs();
        }

        public SchedulerStatus GetStatus()
        {
            var schedulerStatus = new SchedulerStatus
            {
                State = quartzScheduler.InStandbyMode ? SchedulerState.Stopped : SchedulerState.Started,
                ServerTime = DateTime.Now,
            };
            return schedulerStatus;
        }

        public void LoadJobs()
        {
            var jobs = settingsProvider.Load<JobSchedulerSettings>()?.Jobs;
            if (jobs == null)
                throw new ArgumentNullException("JobScheduler settings must define jobs!");

            if (jobs.Count == 0)
            {
                logger.LogWarning("No jobs defined, will not start JobScheduler!");
                return;
            }

            jobDetails.Clear();
            foreach (var job in jobs)
            {
                jobDetails.Add((job, null));
            }
        }

        public SchedulerStatus StartScheduler()
        {
            LoadJobs();
            var activeJobs = jobDetails.Where(i => i.definition.IsActive).ToList();
            if (activeJobs.Count == 0)
            {
                logger.LogWarning("No active jobs defined, JobScheduler has nothing to do!");
            }
            else
            {
                foreach (var job in activeJobs)
                {
                    var trigger = TriggerBuilder
                        .Create()
                        .WithCronSchedule(job.definition.CronExpression, i =>
                            {
                                if (job.definition.FireIfMissed)
                                    i.WithMisfireHandlingInstructionFireAndProceed();
                                else
                                    i.WithMisfireHandlingInstructionDoNothing();
                            })
                        .StartNow();
                    var jobBuilder = JobBuilder
                        .Create(typeResolver.GetByName("job.JobTypeName"))
                        .WithIdentity(job.definition.ID.ToString(), "Cubes")
                        .WithDescription(job.definition.Description);
                    if (!String.IsNullOrEmpty(job.definition.ExecutionParameters))
                        jobBuilder.UsingJobData(PARAMETERS_KEY, job.definition.ExecutionParameters);

                    var scheduledJob = jobBuilder.Build();
                    quartzScheduler.ScheduleJob(scheduledJob, trigger.Build());
                    if (!job.definition.IsActive)
                        quartzScheduler.PauseJob(scheduledJob.Key);
                }
                quartzScheduler.Clear();
                quartzScheduler.Start();
            }
            return GetStatus();
        }

        public SchedulerStatus StopScheduler()
        {
            if (quartzScheduler.IsStarted)
            {
                logger.LogInformation("Stoping JobScheduler.");
                quartzScheduler.Standby();
            }
            return GetStatus();
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