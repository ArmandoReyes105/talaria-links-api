using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Reflection.Metadata;
using System.Text;
using System.Timers;
using talaria_links_api.Common;
using talaria_links_api.DTOs.Tokens;
using talaria_links_api.Entities;
using talaria_links_api.Services.Interfaces;

namespace talaria_links_api.Services.Implementations
{
    public class TokenService(
        IConfiguration configuration,
        TimeProvider time) : ITokenService
    {
        private readonly JsonWebTokenHandler _handler = new();

        public TokenResult CreateToken(Account account)
        {
            var now = time.GetUtcNow().UtcDateTime;
            var expires = now.AddDays(1);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")!));

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = configuration.GetValue<string>("AppSettings:Issuer"),
                Audience = configuration.GetValue<string>("AppSettings:Audience"),
                IssuedAt = now,
                NotBefore = now,
                Expires = expires,
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
                Claims = new Dictionary<string, object>
                {
                    [JwtRegisteredClaimNames.Sub] = account.Id.ToString(),
                    [JwtRegisteredClaimNames.Email] = account.Email,
                    [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString(),
                    [AppClaimTypes.Name] = account.Username,
                    [AppClaimTypes.Role] = account.Role.Name
                }
            };

            return new TokenResult(_handler.CreateToken(descriptor), expires);
        }
    }
}
