using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Security.Cryptography;
using talaria_links_api.Common;
using talaria_links_api.Common.Exceptions;
using talaria_links_api.DTOs.Links;
using talaria_links_api.DTOs.Paging;
using talaria_links_api.Entities;
using talaria_links_api.Enums;
using talaria_links_api.Persistence;
using talaria_links_api.Services.Interfaces;
using LinkOptions = talaria_links_api.Options.LinkOptions;

namespace talaria_links_api.Services.Implementations
{
    public class ShortUrlService(
        TimeProvider time,
        ApplicationDbContext db,
        IOptions<LinkOptions> options) : IShortUrlService
    {
        private const string CodeAlphabet = "abcdefghijkmnpqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        private const int GeneratedCodeLength = 7;
        private const int MaxPageSize = 50; 
        private readonly string _baseUrl = options.Value.BaseUrl.TrimEnd('/');

        public async Task<ShortUrlResponse> GetByIdAsync(int id, CurrentUser user, CancellationToken ct)
        {
            var query = db.ShortUrls.AsNoTracking()
                .Where(s => s.Id == id && (user.IsAdmin || s.AccountId == user.Id));

            return await Project(query).SingleOrDefaultAsync(ct)
                ?? throw new NotFoundException("Link not found.");
        }

        public async Task<PagedResponse<ShortUrlResponse>> GetMineAsync(CurrentUser user, LinkVisibility visibility, int page, int pageSize, CancellationToken ct)
        {
            var query = db.ShortUrls.AsNoTracking().Where(s => s.AccountId == user.Id);

            query = visibility switch
            {
                LinkVisibility.Public => query.Where(s => s.IsPublic),
                LinkVisibility.Private => query.Where(s => !s.IsPublic),
                _ => query
            };

            return await ToPagedAsync(Project(query.OrderByDescending(s => s.CreatedAt)), page, pageSize, ct);
        }

        public async Task<ShortUrlResponse> CreateAsync(CreateShortUrlRequest request, CurrentUser user, CancellationToken ct)
        {
            var link = new ShortUrl
            {
                Code = await GenerateUniqueCodeAsync(ct),
                OriginalUrl = NormalizeUrl(request.OriginalUrl),
                Title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim(),
                IsPublic = request.IsPublic,
                AccountId = user.Id
            };

            db.ShortUrls.Add(link);
            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                throw new ConflictException("Could not generate a unique code. Please try again.");
            }

            return await GetByIdAsync(link.Id, user, ct);
        }

        public async Task<ShortUrlResponse> UpdateAsync(CurrentUser user, int id, UpdateShortUrlRequest request, CancellationToken ct)
        {
            var link = 
                await db.ShortUrls.SingleOrDefaultAsync(s => s.Id == id && s.AccountId == user.Id, ct)
                ?? throw new NotFoundException("Link was not found");

            link.OriginalUrl = NormalizeUrl(request.OriginalUrl);
            link.Title =
                string.IsNullOrWhiteSpace(request.Title)
                ? null
                : request.Title.Trim();
            link.IsPublic = request.IsPublic;
            link.UpdatedAt = time.GetUtcNow().UtcDateTime;

            await db.SaveChangesAsync(ct);
            return await GetByIdAsync(id, user, ct);
        }
        public async Task DeleteAsync(CurrentUser user, int id, CancellationToken ct)
        {
            var link = 
                await db.ShortUrls.SingleOrDefaultAsync(s => s.Id == id && (s.AccountId == user.Id || user.IsAdmin), ct) 
                ?? throw new NotFoundException("Link not found");
            db.ShortUrls.Remove(link);
            await db.SaveChangesAsync(ct);
        }

        public async Task<string> ResolveAndTrackAsync(string code, string? referrer, string? userAgent, CancellationToken ct)
        {
            var link = await db.ShortUrls
                .AsNoTracking()
                .Where(s => s.Code == code)
                .Select(s => new { s.Id, s.OriginalUrl })
                .SingleOrDefaultAsync(ct)
                ?? throw new NotFoundException("Link not found");

            db.Clicks.Add(new Click
            {
                ShortUrlId = link.Id,
                ClickedAt = time.GetUtcNow().UtcDateTime,
                Referrer = Truncate(referrer, 500),
                UserAgent = Truncate(userAgent, 500)
            });

            await db.SaveChangesAsync();

            return link.OriginalUrl;
        }

        private static string? Truncate(string? value, int max) =>
            string.IsNullOrEmpty(value) ? null
            : value.Length <= max ? value
            : value[..max];

        private async Task<string> GenerateUniqueCodeAsync(CancellationToken ct)
        {
            for (var attempt = 0; attempt < 5; attempt++)
            {
                var code = RandomNumberGenerator.GetString(CodeAlphabet, GeneratedCodeLength);
                if (!await db.ShortUrls.AnyAsync(s => s.Code == code, ct))
                    return code;
            }

            throw new ConflictException("Could not generate a unique code. Please try again.");
        }

        private IQueryable<ShortUrlResponse> Project(IQueryable<ShortUrl> query)
        {
            var baseUrl = _baseUrl;
            return query.Select(s => new ShortUrlResponse(
                s.Id,
                s.Code,
                baseUrl + "/r/" + s.Code,
                s.OriginalUrl,
                s.Title,
                s.IsPublic,
                s.Clicks.Count(),
                s.Account.Username,
                s.CreatedAt,
                s.UpdatedAt));
        }

        private static async Task<PagedResponse<T>> ToPagedAsync<T>(IQueryable<T> query, int page, int pageSize, CancellationToken ct)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

            var total = await query.CountAsync(ct);
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            return new PagedResponse<T>(items, page, pageSize, total);
        }

        private async Task<string> ValidateCustomCodeAsync(string code, CancellationToken ct)
        {
            if (await db.ShortUrls.AnyAsync(s => s.Code == code, ct))
                throw new ConflictException("This short code is already taken.");

            return code;
        }

        private static string NormalizeUrl(string raw)
        {
            if (!Uri.TryCreate(raw.Trim(), UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new BadRequestException("The URL must be an absolute http or https address.");
            }

            return uri.AbsoluteUri;
        }
    }
}
