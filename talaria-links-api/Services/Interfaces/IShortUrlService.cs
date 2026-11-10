using talaria_links_api.Common;
using talaria_links_api.DTOs.Links;

namespace talaria_links_api.Services.Interfaces
{
    public interface IShortUrlService
    {
        Task<ShortUrlResponse> CreateAsync(CreateShortUrlRequest request, CurrentUser user, CancellationToken ct);
        Task<ShortUrlResponse> GetByIdAsync(int id, CurrentUser user, CancellationToken ct);
    }
}
