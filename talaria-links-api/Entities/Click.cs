namespace talaria_links_api.Entities
{
    public class Click
    {
        public long Id { get; set; }
        public DateTime ClickedAt { get; set; }
        public string? Referrer { get; set; }
        public string? UserAgent { get; set; }

        public int ShortUrlId { get; set; }
        public ShortUrl ShortUrl { get; set; } = null!; 
    }
}
