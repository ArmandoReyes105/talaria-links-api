using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using talaria_links_api.Common.Exceptions;
using talaria_links_api.DTOs.Auth;
using talaria_links_api.Entities;
using talaria_links_api.Persistence;
using talaria_links_api.Services.Interfaces;

namespace talaria_links_api.Services.Implementations
{
    public class AuthService(
        ApplicationDbContext db,
        IPasswordHasher<Account> hasher,
        ITokenService tokens) : IAuthService
    {
        private const string DefaultRole = "User";
        private const string InvalidCredentials = "Invalid email or password.";
        private static readonly string DummyHash =
        new PasswordHasher<Account>().HashPassword(new Account(), "timing-equalizer-password");

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct)
        {
            var email = NormalizeEmail(request.Email);

            if (await db.Accounts.AnyAsync(a => a.Email == email, ct))
                throw new ConflictException("An account with this email already exists.");

            var role = await db.Roles.SingleAsync(r => r.Name == DefaultRole, ct);

            var account = new Account
            {
                Id = Guid.NewGuid(),
                Email = email,
                Username = request.Username.Trim(),
                Role = role
            };

            account.PasswordHash = hasher.HashPassword(account, request.Password);

            db.Accounts.Add(account);
            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation})
            {
                throw new ConflictException("An account with this email already exists");
            }

            return BuildResponse(account);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct)
        {
            var email = NormalizeEmail(request.Email);
            var account = await db.Accounts
                .Include(a => a.Role)
                .SingleOrDefaultAsync(a => a.Email == email, ct);

            if (account is null)
            {
                hasher.VerifyHashedPassword(new Account(), DummyHash, request.Password);
                throw new UnauthorizedException(InvalidCredentials);
            }

            var result = hasher.VerifyHashedPassword(account, account.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
                throw new UnauthorizedException(InvalidCredentials);

            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                account.PasswordHash = hasher.HashPassword(account, request.Password);
                await db.SaveChangesAsync(ct);
            }

            return BuildResponse(account);
        }

        public async Task<UserResponse> GetCurrentUserAsync(Guid accountId, CancellationToken ct)
        {
            return await db.Accounts
            .AsNoTracking()
            .Where(a => a.Id == accountId)
            .Select(a => new UserResponse(a.Id, a.Email, a.Username, a.Role.Name))
            .SingleOrDefaultAsync(ct)
            ?? throw new NotFoundException("Account not found.");
        }

        private AuthResponse BuildResponse(Account account)
        {
            var token = tokens.CreateToken(account);
            return new AuthResponse(
                token.AccessToken,
                token.ExpiresAt,
                new UserResponse(account.Id, account.Email, account.Username, account.Role.Name));
        }

        private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
    }
}
