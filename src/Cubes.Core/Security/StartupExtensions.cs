using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Cubes.Core.Security
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddCubesAuthentication(
            this IServiceCollection services,
            Action<TokenGeneratorOptions> setupOptions)
        {
            var options = new TokenGeneratorOptions();
            setupOptions?.Invoke(options);
            services.AddSingleton(new TokenGenerator(options));

            if (String.IsNullOrEmpty(options.SecretKey))
                throw new ArgumentException("To use Cubes Authentication, you must set an API key!");

            var key = Encoding.ASCII.GetBytes(options.SecretKey);
            services
                .AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata      = false;
                    x.SaveToken                 = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey         = new SymmetricSecurityKey(key),
                        ValidateIssuer           = false,
                        ValidateAudience         = false
                    };
                });

            services.AddSingleton<SecurityStorage>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddSingleton<InternalAdminPassword>();
            services.AddHostedService<InternalAdminPasswordService>();

            return services;
        }
    }
}
