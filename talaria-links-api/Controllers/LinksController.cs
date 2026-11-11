using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using talaria_links_api.Common;
using talaria_links_api.DTOs.Links;
using talaria_links_api.DTOs.Paging;
using talaria_links_api.Enums;
using talaria_links_api.Services.Interfaces;

namespace talaria_links_api.Controllers
{
    [Route("api/link")]
    [ApiController]
    [Authorize]
    public class LinksController(
        IShortUrlService links) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType<ShortUrlResponse>(StatusCodes.Status201Created)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ShortUrlResponse>> Create([FromBody] CreateShortUrlRequest request, CancellationToken ct)
        {
            var created = await links.CreateAsync(request, User.ToCurrentUser(), ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpGet("mine")]
        [ProducesResponseType<PagedResponse<ShortUrlResponse>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResponse<ShortUrlResponse>>> GetMine(
            [FromQuery] LinkVisibility visibility = LinkVisibility.All,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            var result = await links.GetMineAsync(User.ToCurrentUser(), visibility, page, pageSize, ct);
            return result; 
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType<ShortUrlResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ShortUrlResponse>> GetById(int id, CancellationToken ct)
        => Ok(await links.GetByIdAsync(id, User.ToCurrentUser(), ct));

        [HttpPut("{id:int}")]
        [ProducesResponseType<ShortUrlResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ShortUrlResponse>> Update(int id, UpdateShortUrlRequest request, CancellationToken ct)
        {
            return Ok(await links.UpdateAsync(User.ToCurrentUser(), id, request, ct));
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(int id, CancellationToken ct)
        {
            await links.DeleteAsync(User.ToCurrentUser(), id, ct); 
            return NoContent();
        }
    }
}
