using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Spi;

namespace Cubes.Core.Scheduling
{
    public class SchedulerJobFactory : IJobFactory
    {
        private readonly Dictionary<int, IServiceScope> scopes = new Dictionary<int, IServiceScope>();
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<SchedulerJobFactory> logger;

        public SchedulerJobFactory(IServiceScopeFactory scopeFactory, ILogger<SchedulerJobFactory> logger)
        {
            this.scopeFactory = scopeFactory;
            this.logger       = logger;
        }

        public IJob NewJob(TriggerFiredBundle bundle, Quartz.IScheduler scheduler)
        {
            var scope = scopeFactory.CreateScope();
            var job   = scope.ServiceProvider.GetService(bundle.JobDetail.JobType) as IJob;
            if (job == null)
            {
                logger.LogError($"Could not create job of type: {bundle.JobDetail.JobType}, job will be paused!");
                scheduler.PauseJob(bundle.JobDetail.Key);
                throw new ArgumentException($"Could not create job of type: {bundle.JobDetail.JobType}");
            }
            this.scopes.Add(job.GetHashCode(), scope);

            return job;
        }

        public void ReturnJob(IJob job)
        {
            var key = job.GetHashCode();
            if (scopes.TryGetValue(key, out var scope))
            {
                (job as IDisposable)?.Dispose();
                scope?.Dispose();
                scopes.Remove(key);
            }
        }
    }
}
