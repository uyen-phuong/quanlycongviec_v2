using KHCT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KHCT.Infrastructure.Persistence.Configurations;

public class TaskApprovalHistoryConfiguration : IEntityTypeConfiguration<TaskApprovalHistory>
{
    public void Configure(EntityTypeBuilder<TaskApprovalHistory> b)
    {
        b.ToTable("task_approval_history");
        b.HasKey(x => x.Id);
        b.Property(x => x.Comment).HasMaxLength(2000);
        b.HasIndex(x => x.TaskId);
        b.HasIndex(x => x.DepartmentId);
        b.HasOne(x => x.Task)
            .WithMany(x => x.ApprovalHistories)
            .HasForeignKey(x => x.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ActorUser)
            .WithMany()
            .HasForeignKey(x => x.ActorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
