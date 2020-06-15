using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cubes.Core.Security
{
    public class InternalAdminPasswordService : IHostedService
    {
        private readonly ILogger<InternalAdminPasswordService> logger;
        private readonly InternalAdminPassword adminPassword;

        public InternalAdminPasswordService(ILogger<InternalAdminPasswordService> logger, InternalAdminPassword adminPassword)
        {
            this.logger = logger;
            this.adminPassword = adminPassword;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.logger.LogWarning("Cubes administrator credentials are : cubes / {admin_password}",
                adminPassword.Password);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
