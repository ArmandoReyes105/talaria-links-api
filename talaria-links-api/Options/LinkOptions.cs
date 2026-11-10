using System.ComponentModel.DataAnnotations;

namespace talaria_links_api.Options
{
    public class LinkOptions
    {
        public const string SectionName = "Links";

        [Required, Url] public string BaseUrl { get; set; } = string.Empty;
    }
}
