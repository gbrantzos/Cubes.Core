using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CronExpressionDescriptor;
using Cubes.Core.Base;
using Cubes.Core.Scheduling.ExecutionHistory;
using Cubes.Core.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl.Matchers;
using Quartz.Spi;

namespace Cubes.Core.Scheduling
{
    public class Scheduler : IScheduler
    {
        public static readonly string MessageKey = "CubesScheduling::JobMessage";
        public static readonly string ResultKey  = "CubesScheduling::JobResult";

        private class SchedulerJobDetails
        {
            public string Name                      { get; set; }
            public string CronExpression            { get; set; }
            public bool RefireIfMissed              { get; set; }
            public JobKey JobKey                    { get; set; }
            public Exception LastExecutionException { get; set; }
            public bool LastExecutionFailed         { get; set; }
            public string LastExecutionMessage      { get; set; }
        }

        private readonly IJobFactory _jobFactory;
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IConfigurationRoot _schedulerConfiguration;
        private readonly ILogger<Scheduler> _logger;
        private readonly IExecutionHistoryManager _historyManager;
        private ICollection<SchedulerJobDetails> _internalDetails = new HashSet<SchedulerJobDetails>();

        private Quartz.IScheduler quartzScheduler;
        private bool failedToLoad;

        public Scheduler(
            IJobFactory jobFactory,
            ISchedulerFactory schedulerFactory,
            ILogger<Scheduler> logger,
            IExecutionHistoryManager historyManager,
            ICubesEnvironment cubesEnvironment)
        {
            _schedulerFactory = schedulerFactory;
            _jobFactory = jobFactory;
            _logger = logger;
            _historyManager = historyManager;
            var configurationPath = cubesEnvironment
                .GetFileOnPath(CubesFolderKind.Config, CubesConstants.Files_Scheduling);
            _schedulerConfiguration = new ConfigurationBuilder()
                .AddYamlFile(configurationPath, optional: true, reloadOnChange: true)
                .Build();
        }

        #region IJobScheduler methods
        public async Task Start(CancellationToken cancellationToken = default)
        {
            if (quartzScheduler == null)
            {
                var initialized = await InitialiseScheduler(cancellationToken);
                if (!initialized)
                {
                    _logger.LogError("Failed to initialize scheduler.");
                    return;
                }
            }

            var jobKeys = await quartzScheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
            int allJobs = jobKeys.Count;

            var triggers = await quartzScheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup());
            int activeJobs = triggers.Count;

            _logger.LogInformation($"Configuration loaded, {allJobs} jobs found, {activeJobs} active.");

            await quartzScheduler.Start(cancellationToken);
            if (activeJobs == 0)
                _logger.LogWarning("Scheduler will be started, but has no active jobs!");
            else
                _logger.LogInformation("Scheduler started.");

            if (allJobs > 0)
                _logger.LogInformation(await GetJobsQuickInfo());
        }

        private async Task<string> GetJobsQuickInfo()
        {
            var status = await GetStatus();
            var sb = new StringBuilder();
            sb.Append("Scheduler quick info:");
            sb.AppendLine();
            foreach (var job in status.Jobs)
            {
                sb.Append($"  {job.Name}");
                if (job.Active)
                    sb.Append($" runs { job.CronExpressionDescription}");
                else
                    sb.Append(" is inactive");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public Task Stop(CancellationToken cancellationToken = default)
        {
            if (!quartzScheduler.InStandbyMode)
            {
                _logger.LogInformation("Stopping scheduler...");
                return quartzScheduler.Standby(cancellationToken);
            }
            return Task.CompletedTask;
        }

        public async Task Reload(CancellationToken cancellationToken = default)
        {
            await Stop(cancellationToken);
            var loaded = await LoadConfiguration(cancellationToken);
            if (loaded)
                await Start(cancellationToken);
        }

        public async Task<SchedulerStatus> GetStatus(CancellationToken cancellationToken = default)
        {
            var result = new SchedulerStatus
            {
                ServerTime = DateTime.Now,
                Jobs = new List<SchedulerJobStatus>()
            };
            if (failedToLoad)
            {
                result.SchedulerState = SchedulerStatus.State.FailedToLoad;
                return result;
            }

            if (quartzScheduler.IsStarted)
                result.SchedulerState = SchedulerStatus.State.Active;
            if (quartzScheduler.InStandbyMode)
                result.SchedulerState = SchedulerStatus.State.StandBy;

            var jobKeys = await quartzScheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
            foreach (var key in jobKeys)
            {
                var jobDetail = await quartzScheduler.GetJobDetail(key, cancellationToken);
                var triggers = await quartzScheduler.GetTriggersOfJob(key, cancellationToken);
                var trigger = triggers.FirstOrDefault();

                var internalDetail = _internalDetails.First(dt => dt.JobKey.Equals(jobDetail.Key));
                var name = internalDetail.Name;
                var cronExpression = internalDetail.CronExpression;

                var triggerActive = trigger != null &&
                    (await quartzScheduler.GetTriggerState(trigger.Key, cancellationToken) == TriggerState.Normal);

                var jobStatus = new SchedulerJobStatus
                {
                    Name                 = name,
                    Active               = triggerActive,
                    RefireIfMissed       = internalDetail.RefireIfMissed,
                    JobType              = jobDetail.JobType.FullName,
                    CronExpression       = cronExpression,
                    PreviousFireTime     = trigger?.GetPreviousFireTimeUtc()?.ToLocalTime().DateTime,
                    NextFireTime         = trigger?.GetNextFireTimeUtc()?.ToLocalTime().DateTime,
                    JobParameters        = jobDetail
                        .JobDataMap
                        .ToDictionary(p  => p.Key, p => p.Value.ToString()),
                    LastExecutionFailed  = internalDetail.LastExecutionFailed,
                    LastExecutionMessage = internalDetail.LastExecutionMessage
                };
                if (!String.IsNullOrEmpty(jobStatus.CronExpression))
                {
                    jobStatus.CronExpressionDescription = ExpressionDescriptor
                        .GetDescription(jobStatus.CronExpression, new Options { Use24HourTimeFormat = true })
                        .ToLowerFirstChar();
                }

                result.Jobs.Add(jobStatus);
            }
            result.Jobs.Sort((job1, job2) => job1.Name.CompareTo(job2.Name));

            return result;
        }

        public async Task ExecuteJob(string name, CancellationToken cancellationToken = default)
        {
            var internalDetail = _internalDetails
                .FirstOrDefault(dt => dt.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            if (internalDetail == null)
                throw new ArgumentException($"Unknown job name: {name}");

            var jobDetail = await quartzScheduler.GetJobDetail(internalDetail.JobKey);
            var jobData = new JobDataMap();
            if (jobDetail.JobDataMap != null)
                jobData = (JobDataMap)jobDetail.JobDataMap.Clone();

            await quartzScheduler.TriggerJob(
                new JobKey(jobDetail.Key.Name, jobDetail.Key.Group),
                jobData,
                cancellationToken);
        }
        #endregion

        // Helpers
        private async Task<bool> InitialiseScheduler(CancellationToken cancellationToken = default)
        {
            // First time initialization of Scheduler
            quartzScheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            quartzScheduler.JobFactory = _jobFactory;
            quartzScheduler.ListenerManager.AddJobListener(new JobListener(this));

            // Load configuration
            return await LoadConfiguration(cancellationToken);
        }

        private async Task<bool> LoadConfiguration(CancellationToken cancellationToken = default)
        {
            failedToLoad = true;
            try
            {
                if (!quartzScheduler.InStandbyMode)
                    throw new InvalidOperationException($"{nameof(LoadConfiguration)} should be called when {nameof(Scheduler)} is stopped.");

                // Load configuration
                var settings = _schedulerConfiguration
                    .GetSection(nameof(SchedulerSettings))
                    .Get<SchedulerSettings>();
                if (settings == null)
                    throw new ArgumentException("Scheduler settings cannot be null!");
                await quartzScheduler.Clear();
                _internalDetails = new HashSet<SchedulerJobDetails>();

                // Validate settings
                settings.Validate();

                foreach (var job in settings.Jobs)
                {
                    var details = new SchedulerJobDetails
                    {
                        Name = job.Name,
                        CronExpression = job.CronExpression,
                    };

                    var quartzJob = job.GetQuartzJob();
                    var quartzTrigger = job.GetQuartzTrigger();
                    if (job.Active)
                        await quartzScheduler.ScheduleJob(quartzJob, quartzTrigger, cancellationToken);
                    else
                        await quartzScheduler.AddJob(quartzJob, true, true, cancellationToken);

                    details.JobKey = quartzJob.Key;
                    details.RefireIfMissed = job.RefireIfMissed;
                    _internalDetails.Add(details);
                }

                failedToLoad = false;
                return true;
            }
            catch (Exception x)
            {
                _logger.LogError(x, $"Failed to load scheduler settings: {x.Message}");
                return false;
            }
        }

        internal void AddExecutionResults(JobKey jobKey, Exception exception, string message, bool logicalError = false)
        {
            var internalDetail = _internalDetails
                .FirstOrDefault(dt => dt.JobKey.Equals(jobKey));
            if (internalDetail == null)
                throw new ArgumentException($"Invalid job key: {jobKey}");

            internalDetail.LastExecutionException = exception;
            internalDetail.LastExecutionFailed = logicalError || (exception != null);
            if (exception != null)
            {
                var allMesages = exception.GetAllMessages();
                internalDetail.LastExecutionMessage = String.Join(Environment.NewLine, allMesages);
            }
            else
                internalDetail.LastExecutionMessage = String.IsNullOrEmpty(message)
                    ? "Execution was successful!"
                    : message;
        }

        private class JobListener : IJobListener
        {
            private readonly Scheduler scheduler;

            public JobListener(Scheduler scheduler) => this.scheduler = scheduler;

            public string Name => "Internal Job Listener [Cubes.Core]";

            public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
                => Task.CompletedTask;

            public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
                => Task.CompletedTask;

            public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = default)
            {
                string message = context.SafeGet(Scheduler.MessageKey)?.ToString();
                string tmp = context.SafeGet(Scheduler.ResultKey)?.ToString();
                bool logicalError = String.IsNullOrEmpty(tmp)? false : tmp.Equals(Boolean.TrueString);

                if (String.IsNullOrEmpty(message) && context.Result != null)
                    message = context.Result?.ToString();
                if (String.IsNullOrEmpty(message))
                    message = "Job executed successfully, but no details are available!";
                scheduler.AddExecutionResults(context.JobDetail.Key, jobException, message, logicalError);
                return Task.CompletedTask;
            }
        }
    }

    public static class JobExecutionContextExtensions
    {
        public static object SafeGet(this IJobExecutionContext executionContext, string key)
        {
            try
            {
                return executionContext.Get(key);
            }
            catch
            {
                //No big deal, just swallow and continue
                return null;
            }
        }
    }
}
