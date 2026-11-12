using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using talaria_links_api.Services.Interfaces;

namespace talaria_links_api.Controllers
{
    [Route("r")]
    [ApiController]
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class RedirectController(IShortUrlService linksService) : ControllerBase
    {
        [HttpGet("{code:maxlength(32)}")]
        public async Task<IActionResult> Follow(string code, CancellationToken ct)
        {
            var url = await linksService.ResolveAndTrackAsync(
                code,
                Request.Headers.Referer.ToString(),
                Request.Headers.UserAgent.ToString(),
                ct);

            return Redirect(url);
        }
    }
}
