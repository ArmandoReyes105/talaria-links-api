using Microsoft.EntityFrameworkCore;

namespace talaria_links_api.DTOs.Paging
{
    public static class PagingExtensions
    {
        public const int MaxPageSize = 50;

        public static async Task<PagedResponse<T>> ToPagedAsync<T>(
            this IQueryable<T> query, int page, int pageSize, CancellationToken ct)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

            var total = await query.CountAsync(ct);
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            return new PagedResponse<T>(items, page, pageSize, total);
        }
    }
}
