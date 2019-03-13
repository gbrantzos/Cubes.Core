using Microsoft.Extensions.DependencyInjection;

namespace Cubes.Core.Email
{
    public static class StartupExtensions
    {
        public static void AddEmailDispatcher(this IServiceCollection services)
            => services.AddScoped<IEmailDispatcher>(c => new EmailDispatcher(new SmtpClientWrapper()));
    }
}