using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using talaria_links_api.Common;
using talaria_links_api.DTOs.Admin;
using talaria_links_api.DTOs.Links;
using talaria_links_api.DTOs.Paging;
using talaria_links_api.Services.Interfaces;

namespace talaria_links_api.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = RoleNames.Admin)]
    public class AdminController(
        IAccountService accounts,
        IShortUrlService links) : ControllerBase
    {
        [HttpGet("users")]
        [ProducesResponseType<PagedResponse<AdminUserResponse>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResponse<AdminUserResponse>>> GetUsers(
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            return await accounts.GetAllAsync(search, page, pageSize, ct);
        }

        [HttpPut("users/{id:guid}/role")]
        [ProducesResponseType<AdminUserResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<AdminUserResponse>> UpdateUserRole(
            Guid id,
            UpdateUserRoleRequest request,
            CancellationToken ct)
        {
            return Ok(await accounts.UpdateRoleAsync(User.ToCurrentUser(), id, request.Role, ct));
        }

        [HttpGet("links")]
        [ProducesResponseType<PagedResponse<ShortUrlResponse>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResponse<ShortUrlResponse>>> GetLinks(
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            return await links.GetAllAsync(search, page, pageSize, ct);
        }
    }
}
