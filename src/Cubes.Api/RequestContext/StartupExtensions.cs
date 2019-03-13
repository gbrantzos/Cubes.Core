using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Cubes.Api.RequestContext
{
    public static class Startupextensions
    {
        public static void AddContextProvider(this IServiceCollection services)
        {
            services.AddScoped<IContextProvider, ContextProvider>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        public static IApplicationBuilder UseContextProvider(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                var ctxProvider = context.RequestServices.GetService<IContextProvider>();
                ctxProvider.Current = Context.FromHttpContext(context);

                await next.Invoke();
            });
            return app;
        }
    }
}