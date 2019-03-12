using System;
using Quartz;
using Quartz.Simpl;
using Quartz.Spi;

namespace Cubes.Core.Jobs
{
    public class CubesJobFactory : SimpleJobFactory
    {
        private readonly IServiceProvider serviceProvider;
        public CubesJobFactory(IServiceProvider serviceProvider)
            => this.serviceProvider = serviceProvider;

        public override IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
            => serviceProvider.GetService(bundle.JobDetail.JobType) as IJob;
    }
}