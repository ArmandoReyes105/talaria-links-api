using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using talaria_links_api.Common;
using talaria_links_api.Entities;

namespace talaria_links_api.Persistence
{
    public static class AdminSeeder
    {
        public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
        {
            var email = config["AdminSeed:Email"];
            var password = config["AdminSeed:Password"];

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return;

            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Account>>();

            if (await db.Accounts.AnyAsync(a => a.Role.Name == RoleNames.Admin))
                return;

            var normalizedEmail = email.Trim().ToLowerInvariant();
            if (await db.Accounts.AnyAsync(a => a.Email == normalizedEmail))
                return;

            var adminRole = await db.Roles.SingleAsync(r => r.Name == RoleNames.Admin);

            var admin = new Account
            {
                Id = Guid.NewGuid(),
                Email = normalizedEmail,
                Username = config["AdminSeed:Username"] ?? "admin",
                Role = adminRole,
            };
            admin.PasswordHash = hasher.HashPassword(admin, password);

            db.Accounts.Add(admin);
            await db.SaveChangesAsync();
        }
    }
}
