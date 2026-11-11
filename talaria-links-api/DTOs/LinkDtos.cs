using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace talaria_links_api.DTOs.Links
{
    public sealed record CreateShortUrlRequest(
    [Required, MaxLength(2048)] string OriginalUrl,
    [MaxLength(150)] string? Title,
    bool IsPublic = false);

    public sealed record UpdateShortUrlRequest(
        [Required, MaxLength(2048)] string OriginalUrl,
        [MaxLength(150)] string? Title,
        bool IsPublic);

    public sealed record ShortUrlResponse(
    int Id,
    string Code,
    string ShortUrl,
    string OriginalUrl,
    string? Title,
    bool IsPublic,
    int ClickCount,
    string OwnerName,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
}
