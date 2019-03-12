using System.Collections.Specialized;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Simpl;

namespace Cubes.Core.Jobs
{
    public static class Extensions
    {
        public static void AddJobScheduler(this IServiceCollection services)
        {
            services.AddTransient<CubesJobFactory>();
            services.AddSingleton<IScheduler>(s =>
            {
                var properties = new NameValueCollection
                {
                    { "quartz.scheduler.instanceName", "CubesScheduler" },
                    { "quartz.threadPool.type", "Quartz.Simpl.SimpleThreadPool, Quartz" },
                    { "quartz.threadPool.threadCount", "4" },
                    { "quartz.jobStore.misfireThreshold", "60000" },
                    { "quartz.serializer.type", "json" }
                };
                var schedulerFactory = new StdSchedulerFactory(properties);
                var scheduler        = schedulerFactory.GetScheduler().Result;
                scheduler.JobFactory = s.GetService<CubesJobFactory>();

                return scheduler;
            });
            services.AddSingleton<IJobScheduler, JobScheduler>();
        }
    }
}