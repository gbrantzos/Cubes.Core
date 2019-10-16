using System;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace Cubes.Core.Scheduling
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddScheduler(this IServiceCollection services,
            NameValueCollection quartzProperties = null,
            params Assembly[] assemblies)
        {
            // Add default properties for Quartz
            var defaults = new NameValueCollection
            {
                { "quartz.scheduler.instanceName"   , "Quartz Scheduler"                      },
                { "quartz.threadPool.type"          , "Quartz.Simpl.SimpleThreadPool, Quartz" },
                { "quartz.threadPool.threadCount"   , "4"                                     },
                { "quartz.jobStore.misfireThreshold", "60000"                                 },
                { "quartz.serializer.type"          , "binary"                                },
            };
            if (quartzProperties == null)
                quartzProperties = defaults;
            foreach (string key in defaults)
            {
                if (String.IsNullOrEmpty(quartzProperties.Get(key)))
                    quartzProperties.Add(key, defaults.Get(key));
            }

            // Add Quartz services
            services.AddSingleton<IJobFactory, SchedulerJobFactory>();
            services.AddSingleton<ISchedulerFactory>(new StdSchedulerFactory(quartzProperties));

            // Hosted Service
            services.AddHostedService<SchedulerHostedService>();

            // Jobs scheduler
            services.AddSingleton<IScheduler, Scheduler>();

            // Add jobs
            if (assemblies?.Length>0)
            {
                var jobs = assemblies
                    .SelectMany(a => a.GetTypes())
                    .Where(t => t.GetInterfaces().Contains(typeof(Quartz.IJob)) && !t.IsAbstract)
                    .ToList();
                foreach (var jobType in jobs)
                    services.AddTransient(jobType);
            }

            return services;
        }

        public static IServiceCollection AddScheduler(this IServiceCollection services, params Assembly[] assemblies)
            => services.AddScheduler(null, assemblies);
    }
}
