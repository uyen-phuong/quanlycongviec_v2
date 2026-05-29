using KHCT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KHCT.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.ToTable("audit_log");
        b.HasKey(x => x.Id);
        b.Property(x => x.EntityName).HasMaxLength(128).IsRequired();
        b.Property(x => x.Action).HasMaxLength(32).IsRequired();
        b.Property(x => x.BeforeJson).HasColumnType("json");
        b.Property(x => x.AfterJson).HasColumnType("json");
        b.HasIndex(x => new { x.EntityName, x.EntityId });
        b.HasOne(x => x.ActorUser)
            .WithMany()
            .HasForeignKey(x => x.ActorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
