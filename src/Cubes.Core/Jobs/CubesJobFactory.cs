using System;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Simpl;
using Quartz.Spi;

namespace Cubes.Core.Jobs
{
    public class CubesJobFactory : SimpleJobFactory, IDisposable
    {
        private readonly IServiceScope scope;
        public CubesJobFactory(IServiceProvider serviceProvider)
            => this.scope = serviceProvider.CreateScope();

        public void Dispose()
            => scope.Dispose();

        public override IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
            => scope.ServiceProvider.GetService(bundle.JobDetail.JobType) as IJob;

        public override void ReturnJob(IJob job)
            => (job as IDisposable)?.Dispose();
    }
}