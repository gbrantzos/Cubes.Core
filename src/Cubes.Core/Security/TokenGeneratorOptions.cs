using System;

namespace Cubes.Core.Security
{
    public class TokenGeneratorOptions
    {
        public string SecretKey { get; set; }
        public TimeSpan TokenLifetime { get; set; } = TimeSpan.FromHours(2);
    }
}