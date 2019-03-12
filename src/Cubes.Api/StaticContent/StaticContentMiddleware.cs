using System.Threading.Tasks;
using Cubes.Core.Settings;
using Microsoft.AspNetCore.Http;

namespace Cubes.Api.StaticContent
{
    public class StaticContentMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ISettingsProvider settingsProvider;

        public StaticContentMiddleware(RequestDelegate next, ISettingsProvider settingsProvider)
        {
            this.next = next;
            this.settingsProvider = settingsProvider;
        }

        public async Task Invoke(HttpContext ctx) => await next(ctx);
    }
}