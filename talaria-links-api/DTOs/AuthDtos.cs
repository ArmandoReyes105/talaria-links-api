using System.ComponentModel.DataAnnotations;

namespace talaria_links_api.DTOs.Auth
{
    public sealed record RegisterRequest(
        [Required, MaxLength(100)] 
        string Username,

        [Required, EmailAddress, MaxLength(256)] 
        string Email,

        [Required, MinLength(8), MaxLength(100),
        RegularExpression(@"^(?=.*[A-Z])(?=.*\d).+$",
            ErrorMessage = "Password must contain at least one uppercase letter and one number.")]
        string Password);

    public sealed record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

    public sealed record UserResponse(
        Guid Id,
        string Email,
        string Username,
        string Role);

    public sealed record AuthResponse(
        string AccessToken,
        DateTime ExpiresAt,
        UserResponse User);
}
