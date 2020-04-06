using System;
using System.Collections.Generic;
using System.Linq;
using Cubes.Core.Base;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cubes.Core.Web
{
    public class CorsPolicy
    {
        public string PolicyName { get; set; } = String.Empty;
        public string[] Origins { get; set; }
        public string[] Methods { get; set; }
        public string[] Headers { get; set; }
    }

    public static class CorsConfigurationExtensions
    {
        public static IEnumerable<CorsPolicy> GetCorsPolicies(this IConfiguration configuration)
        {
            var policies = configuration
                .GetSection(CubesConstants.Config_HostCorsPolicies)
                .Get<CorsPolicy[]>();

            return policies ?? Enumerable.Empty<CorsPolicy>();
        }

        public static IServiceCollection AddCorsConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            foreach (var policy in configuration.GetCorsPolicies())
            {
                services.AddCors(options =>
                {
                    if (String.IsNullOrEmpty(policy.PolicyName))
                        options.AddDefaultPolicy(builder => builder.AddCorsConfiguration(policy));
                    else
                        options.AddPolicy(policy.PolicyName, builder => builder.AddCorsConfiguration(policy));
                });
            }
            return services;
        }

        private static CorsPolicyBuilder AddCorsConfiguration(this CorsPolicyBuilder builder, CorsPolicy configuration)
        {
            builder.WithOrigins(configuration.Origins);
            if (configuration.Methods?.Length > 0)
                builder.WithMethods(configuration.Methods);
            if (configuration.Headers?.Length > 0)
                builder.WithMethods(configuration.Headers);

            return builder;
        }
    }
}
