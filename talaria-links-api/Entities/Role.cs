namespace talaria_links_api.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<Account> Accounts { get; set; } = [];
    }
}
