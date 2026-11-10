using talaria_links_api.DTOs.Auth;

namespace talaria_links_api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct);
        Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct);
        Task<UserResponse> GetCurrentUserAsync(Guid accountId, CancellationToken ct);
    }
}
