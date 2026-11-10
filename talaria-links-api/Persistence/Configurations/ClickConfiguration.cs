using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using talaria_links_api.Entities;

namespace talaria_links_api.Persistence.Configurations
{
    public class ClickConfiguration : IEntityTypeConfiguration<Click>
    {
        public void Configure(EntityTypeBuilder<Click> builder)
        {
            builder.ToTable("Clicks");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.ClickedAt).HasDefaultValueSql("now()");
            builder.Property(c => c.Referrer).HasMaxLength(500);
            builder.Property(c => c.UserAgent).HasMaxLength(500);

            builder.HasIndex(c => new { c.ShortUrlId, c.ClickedAt });

            builder.HasOne(c => c.ShortUrl)
                .WithMany(s => s.Clicks)
                .HasForeignKey(c => c.ShortUrlId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
