using Microsoft.AspNetCore.Mvc.RazorPages;
using talaria_links_api.Common;
using talaria_links_api.DTOs.Links;
using talaria_links_api.DTOs.Paging;
using talaria_links_api.Enums;

namespace talaria_links_api.Services.Interfaces
{
    public interface IShortUrlService
    {
        Task<ShortUrlResponse> CreateAsync(CreateShortUrlRequest request, CurrentUser user, CancellationToken ct);
        Task<ShortUrlResponse> GetByIdAsync(int id, CurrentUser user, CancellationToken ct);
        Task<PagedResponse<ShortUrlResponse>> GetMineAsync(CurrentUser user, LinkVisibility visibility, int page, int pageSize, CancellationToken ct);
        Task DeleteAsync(CurrentUser user, int id, CancellationToken ct);
        Task<ShortUrlResponse> UpdateAsync(CurrentUser user, int id, UpdateShortUrlRequest request, CancellationToken ct);
    }
}
