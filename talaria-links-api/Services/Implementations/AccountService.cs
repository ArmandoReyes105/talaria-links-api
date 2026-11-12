using Microsoft.EntityFrameworkCore;
using talaria_links_api.Common;
using talaria_links_api.Common.Exceptions;
using talaria_links_api.DTOs.Admin;
using talaria_links_api.DTOs.Paging;
using talaria_links_api.Entities;
using talaria_links_api.Persistence;
using talaria_links_api.Services.Interfaces;

namespace talaria_links_api.Services.Implementations
{
    public class AccountService(ApplicationDbContext db) : IAccountService
    {
        public async Task<PagedResponse<AdminUserResponse>> GetAllAsync(string? search, int page, int pageSize, CancellationToken ct)
        {
            var query = db.Accounts.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = LikePattern.Contains(search);
                query = query.Where(a =>
                    EF.Functions.ILike(a.Username, pattern, LikePattern.EscapeChar) ||
                    EF.Functions.ILike(a.Email, pattern, LikePattern.EscapeChar));
            }

            return await Project(query.OrderBy(a => a.Username).ThenBy(a => a.Id))
                .ToPagedAsync(page, pageSize, ct);
        }

        public async Task<AdminUserResponse> UpdateRoleAsync(CurrentUser actor, Guid userId, string roleName, CancellationToken ct)
        {
            if (userId == actor.Id)
                throw new ConflictException("You cannot change your own role.");

            var role = await db.Roles.SingleOrDefaultAsync(r => r.Name == roleName, ct)
                ?? throw new BadRequestException("Invalid role.");

            var account = await db.Accounts.SingleOrDefaultAsync(a => a.Id == userId, ct)
                ?? throw new NotFoundException("User not found.");

            if (account.RoleId != role.Id)
            {
                account.RoleId = role.Id;
                await db.SaveChangesAsync(ct);
            }

            return await Project(db.Accounts.AsNoTracking().Where(a => a.Id == userId))
                .SingleAsync(ct);
        }

        private static IQueryable<AdminUserResponse> Project(IQueryable<Account> query) =>
            query.Select(a => new AdminUserResponse(
                a.Id,
                a.Username,
                a.Email,
                a.Role.Name,
                a.ShortUrls.Count(),
                a.CreatedAt));
    }
}
