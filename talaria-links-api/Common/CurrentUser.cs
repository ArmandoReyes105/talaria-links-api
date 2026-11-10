using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using talaria_links_api.Common.Exceptions;

namespace talaria_links_api.Common
{
    public sealed record CurrentUser(Guid Id, bool IsAdmin);
    public static class ClaimsPrinciipalExtensions
    {
        public static CurrentUser ToCurrentUser(this ClaimsPrincipal principal)
        {
            var sub = principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(sub, out var id))
                throw new UnauthorizedException("Invalid token.");

            return new CurrentUser(id, principal.IsInRole(RoleNames.Admin));
        }
    }
}
