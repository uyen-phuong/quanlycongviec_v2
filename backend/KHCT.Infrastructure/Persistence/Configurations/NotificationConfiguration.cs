using KHCT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KHCT.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> b)
    {
        b.ToTable("notification");
        b.HasKey(x => x.Id);
        b.Property(x => x.Title).HasMaxLength(255).IsRequired();
        b.Property(x => x.Body).HasColumnType("text");
        b.Property(x => x.EventType).HasMaxLength(64).IsRequired();
        b.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAt });
        b.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
