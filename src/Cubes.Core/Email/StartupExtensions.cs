using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cubes.Core.Email
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddEmailDispatcher(this IServiceCollection services, IConfiguration config)
            => services
                .AddTransient<ISmtpClient, MailKitSmtpClient>()
                .AddScoped<IEmailDispatcher, EmailDispatcher>()
                .Configure<SmtpSettingsProfiles>(config.GetSection(nameof(SmtpSettingsProfiles)));
    }
}