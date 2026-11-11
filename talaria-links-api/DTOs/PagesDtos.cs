namespace talaria_links_api.DTOs.Paging
{
    public sealed record PagedResponse<T>(
        IReadOnlyList<T> items,
        int Page,
        int PageSize,
        int TotalCount)
    {
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
