using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Cubes.Core.Scheduling
{
    public class SchedulerHostedService : IHostedService
    {
        private readonly IScheduler scheduler;

        public SchedulerHostedService(IScheduler scheduler)
            => this.scheduler = scheduler;

        public async Task StartAsync(CancellationToken cancellationToken)
            => await this.scheduler.Start(cancellationToken);

        public async Task StopAsync(CancellationToken cancellationToken)
            => await this.scheduler.Stop(cancellationToken);
    }
}
