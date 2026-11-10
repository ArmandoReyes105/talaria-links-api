using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using talaria_links_api.Entities;

namespace talaria_links_api.Persistence.Configurations
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.ToTable("Accounts");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Email)
                .HasMaxLength(256).IsRequired();
            builder.Property(a => a.Username)
                .HasMaxLength(100).IsRequired();
            builder.Property(a => a.CreatedAt)
                .HasDefaultValueSql("now()");

            builder.HasOne(a => a.Role)
                .WithMany(r => r.Accounts)
                .HasForeignKey(a => a.RoleId)
                .OnDelete(DeleteBehavior.Restrict); 
        }
    }
}
