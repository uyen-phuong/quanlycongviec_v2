using KHCT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KHCT.Infrastructure.Persistence.Configurations;

public class ApprovalHistoryConfiguration : IEntityTypeConfiguration<ApprovalHistory>
{
    public void Configure(EntityTypeBuilder<ApprovalHistory> b)
    {
        b.ToTable("approval_history");
        b.HasKey(x => x.Id);
        b.Property(x => x.Comment).HasMaxLength(2000);
        b.HasIndex(x => x.PlanId);
        b.HasOne(x => x.Plan)
            .WithMany()
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.ActorUser)
            .WithMany()
            .HasForeignKey(x => x.ActorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
