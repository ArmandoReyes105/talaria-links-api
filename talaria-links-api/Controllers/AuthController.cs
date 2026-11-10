using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using talaria_links_api.DTOs.Auth;
using talaria_links_api.Services.Interfaces;

namespace talaria_links_api.Controllers
{
    [Route("api/auth")]
    [Controller]
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
            return CreatedAtAction(nameof(Me), null, response);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponse>> Login([FromBody]LoginRequest request, CancellationToken ct)
        => Ok(await auth.LoginAsync(request, ct));

        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserResponse>> Me(CancellationToken ct)
        {
            var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (!Guid.TryParse(sub, out var accountId))
                return Unauthorized();

            return Ok(await auth.GetCurrentUserAsync(accountId, ct));
        }
    }
}
