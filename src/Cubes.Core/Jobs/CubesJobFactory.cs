using System;
using Microsoft.Extensions.DependencyInjection;
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
        {
            using(var scope = serviceProvider.CreateScope())
            {
                var job = scope.ServiceProvider.GetRequiredService(bundle.JobDetail.JobType);
                return job as IJob;
            }
        }
    }
}