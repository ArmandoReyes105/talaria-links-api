using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using talaria_links_api.Entities;

namespace talaria_links_api.Persistence.Configurations
{
    public class ShortUrlConfiguration : IEntityTypeConfiguration<ShortUrl>
    {
        public void Configure(EntityTypeBuilder<ShortUrl> builder)
        {
            builder.ToTable("ShortUrls");
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Code)
                .HasMaxLength(32).IsUnicode(false).IsRequired();
            builder.HasIndex(s => s.Code)
                .IsUnique();

            builder.Property(s => s.OriginalUrl)
                .HasMaxLength(2048).IsRequired();
            builder.Property(s => s.Title).HasMaxLength(150);
            builder.Property(s => s.IsPublic).HasDefaultValue(false);
            builder.Property(s => s.CreatedAt)
                .HasDefaultValueSql("now()");

            builder.HasIndex(s => new { s.IsPublic, s.CreatedAt });

            builder.HasOne(s => s.Account)
                .WithMany(a => a.ShortUrls)
                .HasForeignKey(s => s.AccountId)
                .OnDelete(DeleteBehavior.Cascade); 
        }
    }
}
