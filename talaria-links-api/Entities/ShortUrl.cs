namespace talaria_links_api.Entities
{
    public class ShortUrl
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string OriginalUrl { get; set; } = string.Empty;
        public string? Title { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Guid AccountId { get; set; }
        public Account Account { get; set; } = null!;

        public ICollection<Click> Clicks { get; set; } = []; 
    }
}
