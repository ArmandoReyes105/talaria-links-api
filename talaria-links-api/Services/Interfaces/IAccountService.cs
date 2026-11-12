using talaria_links_api.Common;
using talaria_links_api.DTOs.Admin;
using talaria_links_api.DTOs.Paging;

namespace talaria_links_api.Services.Interfaces
{
    public interface IAccountService
    {
        Task<PagedResponse<AdminUserResponse>> GetAllAsync(string? search, int page, int pageSize, CancellationToken ct);
        Task<AdminUserResponse> UpdateRoleAsync(CurrentUser actor, Guid userId, string roleName, CancellationToken ct);
    }
}
