namespace talaria_links_api.Entities
{
    public class Account
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!; 
        public DateTime CreatedAt { get; set; }
        public ICollection<ShortUrl> ShortUrls { get; set; } = [];
    }
}
