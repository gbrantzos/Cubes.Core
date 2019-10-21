using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cubes.Core.Email
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddEmailDispatcher(this IServiceCollection services, IConfiguration config)
            => services
                .AddScoped<IEmailDispatcher>(c => new EmailDispatcher(new SmtpClientWrapper()))
                .Configure<SmtpSettingsProfiles>(config.GetSection(nameof(SmtpSettingsProfiles)));
    }
}