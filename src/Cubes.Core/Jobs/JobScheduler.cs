using System;
using System.Collections.Generic;
using Cubes.Core.Settings;
using Quartz;

namespace Cubes.Core.Jobs
{
    public class JobScheduler : IJobScheduler
    {
        private readonly ISettingsProvider settingsProvider;
        private readonly List<(JobDefinition definition, JobExecution execution)> jobDetails;
        private readonly IScheduler quartzScheduler;

        public JobScheduler(ISettingsProvider settingsProvider, IScheduler quartzScheduler)
        {
            this.settingsProvider = settingsProvider;
            this.quartzScheduler = quartzScheduler;

            this.jobDetails = new List<(JobDefinition definition, JobExecution execution)>();
            this.LoadJobs();
        }

        public SchedulerStatus GetStatus()
        {
            throw new System.NotImplementedException();
        }

        public void LoadJobs()
        {
            var jobs = settingsProvider.Load<JobSchedulerSettings>()?.Jobs;
            if (jobs == null)
                throw new ArgumentNullException("JobScheduler settings must define jobs!");

            if (jobs.Count == 0)
            {
                // Log end exit
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
            quartzScheduler.Start();
            return null;
        }

        public SchedulerStatus StopScheduler()
        {
            quartzScheduler.Shutdown();
            return null;
        }
    }
}