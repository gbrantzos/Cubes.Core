using System;
using System.Collections.Generic;
using System.Linq;
using Cubes.Core.Scheduling.Jobs;
using Quartz;

namespace Cubes.Core.Scheduling
{
    public class SchedulerSettings
    {
        public ICollection<SchedulerJob> Jobs { get; set; } = new HashSet<SchedulerJob>();

        public void Validate()
        {
            var duplicateNames = Jobs
                .GroupBy(job => job.Name)
                .Select(group => new
                {
                    Name = group.Key,
                    Count = group.Count()
                })
                .Where(group => group.Count > 1)
                .Select(group => group.Name)
                .ToArray();
            if (duplicateNames.Length > 0)
                throw new ArgumentException($"Duplicate job names: {String.Join(',', duplicateNames)}");
            foreach (var job in Jobs)
                job.Validate();
        }

        public static SchedulerSettings Create()
        {
            var toReturn = new SchedulerSettings();
            toReturn.Jobs.Add(new SchedulerJob
            {
                Name           = "Sample Job",
                Active         = false,
                JobType        = typeof(SampleJob).FullName,
                CronExpression = "0/45 * * * * ?",
            });
            return toReturn;
        }
    }

    public class SchedulerJob
    {
        public string Name                           { get; set; }
        public bool Active                           { get; set; } = true;
        public string CronExpression                 { get; set; }
        public string JobType                        { get; set; }
        public Dictionary<string, string> Parameters { get; set; }

        public void Validate()
        {
            if (String.IsNullOrEmpty(Name))
                throw new ArgumentNullException(nameof(Name));
            if (String.IsNullOrEmpty(CronExpression))
                throw new ArgumentNullException(nameof(CronExpression));
            if (String.IsNullOrEmpty(JobType))
                throw new ArgumentNullException(nameof(JobType));
        }

        public IJobDetail GetQuartzJob()
        {
            Type jobType = GetJobType();
            var jobBuilder = JobBuilder
                .Create(jobType)
                .StoreDurably()
                .WithIdentity($"{Name} - {jobType.FullName}")
                .WithDescription($"{Name}");
            if (Parameters?.Count > 0)
            {
                var jobData = new JobDataMap();
                foreach (var item in Parameters)
                    jobData.Add(item.Key, item.Value);
                jobBuilder.SetJobData(jobData);
            }

            return jobBuilder.Build();
        }

        public ITrigger GetQuartzTrigger()
        {
            Type jobType = GetJobType();
            return TriggerBuilder
                .Create()
                .WithIdentity($"{Name} - {jobType.FullName}.Trigger")
                .WithDescription(CronExpression)
                .WithCronSchedule(CronExpression)
                .Build();
        }

        private Type GetJobType()
        {
            if (String.IsNullOrEmpty(JobType))
                throw new ArgumentException($"JobType not defined for: {Name}");

            var jobType = Type.GetType(JobType);
            if (jobType == null)
                throw new ArgumentException($"Could not create type for job: {JobType}");

            return jobType;
        }
    }
}
