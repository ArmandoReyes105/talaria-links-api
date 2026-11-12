using System.ComponentModel.DataAnnotations;

namespace talaria_links_api.DTOs.Admin
{
    public sealed record AdminUserResponse(
        Guid Id,
        string Username,
        string Email,
        string Role,
        int LinkCount,
        DateTime CreatedAt);

    public sealed record UpdateUserRoleRequest(
        [Required] string Role);
}
