using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using talaria_links_api.Common;
using talaria_links_api.DTOs.Links;
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

        [HttpGet("{id:int}")]
        [ProducesResponseType<ShortUrlResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ShortUrlResponse>> GetById(int id, CancellationToken ct)
        => Ok(await links.GetByIdAsync(id, User.ToCurrentUser(), ct));
    }
}
