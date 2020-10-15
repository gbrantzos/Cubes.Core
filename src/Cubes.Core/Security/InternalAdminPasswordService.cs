using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cubes.Core.Security
{
    public class InternalAdminPasswordService : IHostedService
    {
        private readonly ILogger<InternalAdminPasswordService> _logger;
        private readonly InternalAdminPassword _adminPassword;

        public InternalAdminPasswordService(ILogger<InternalAdminPasswordService> logger, InternalAdminPassword adminPassword)
        {
            _logger = logger;
            _adminPassword = adminPassword;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Cubes administrator credentials are : cubes / {admin_password}", _adminPassword.Password);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
