using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Cubes.Core.Security
{
    public class TokenGenerator
    {
        private readonly TokenGeneratorOptions tokenGeneratorOptions;

        public TokenGenerator(TokenGeneratorOptions tokenGeneratorOptions)
            => this.tokenGeneratorOptions = tokenGeneratorOptions;

        public string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(tokenGeneratorOptions.SecretKey);

            var claims = new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
                new Claim(ClaimTypes.Name, user.DisplayName)
            };
            var roles = user
                .Roles
                .Select(r => new Claim(ClaimTypes.Role, r.Code));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims.Concat(roles)),
                Expires = DateTime.UtcNow.Add(tokenGeneratorOptions.TokenLifetime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
