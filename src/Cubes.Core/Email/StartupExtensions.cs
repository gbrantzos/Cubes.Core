using Microsoft.Extensions.DependencyInjection;

namespace Cubes.Core.Email
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddEmailDispatcher(this IServiceCollection services)
            => services.AddScoped<IEmailDispatcher>(c => new EmailDispatcher(new SmtpClientWrapper()));
    }
}