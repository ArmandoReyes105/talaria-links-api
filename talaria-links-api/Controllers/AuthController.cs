using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using talaria_links_api.Common;
using talaria_links_api.DTOs.Auth;
using talaria_links_api.Services.Interfaces;

namespace talaria_links_api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController(IAuthService auth) : ControllerBase
    {
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType<AuthResponse>(StatusCodes.Status201Created)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<AuthResponse>> Register([FromBody]RegisterRequest request, CancellationToken ct)
        {
            var response = await auth.RegisterAsync(request, ct);
            SetAuthCookie(response.AccessToken, response.ExpiresAt);
            return CreatedAtAction(nameof(Me), null, response);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponse>> Login([FromBody]LoginRequest request, CancellationToken ct)
        {
            var result = await auth.LoginAsync(request, ct);
            SetAuthCookie(result.AccessToken, result.ExpiresAt);
            return Ok(result);
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult Logout()
        {
            Response.Cookies.Delete(AuthCookie.Name, BuildCookieOptions());
            return NoContent();
        }

        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserResponse>> Me(CancellationToken ct)
        {
            var user = User.ToCurrentUser();
            return Ok(await auth.GetCurrentUserAsync(user.Id, ct));
        }

        private void SetAuthCookie(string token, DateTime expiresAt)
        {
            var options = BuildCookieOptions();
            options.Expires = expiresAt;
            Response.Cookies.Append(AuthCookie.Name, token, options);
        }

        private static CookieOptions BuildCookieOptions() => new()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/"
        };
    }
}
