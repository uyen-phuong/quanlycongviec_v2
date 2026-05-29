using KHCT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskEntity = KHCT.Domain.Entities.Task;

namespace KHCT.Infrastructure.Persistence.Configurations;

public class TaskConfiguration : IEntityTypeConfiguration<TaskEntity>
{
    public void Configure(EntityTypeBuilder<TaskEntity> b)
    {
        b.ToTable("task");
        b.HasKey(x => x.Id);
        b.Property(x => x.Title).HasMaxLength(1024).IsRequired();
        b.Property(x => x.OutlineIndex).HasMaxLength(64);
        b.Property(x => x.BksMemberText).HasMaxLength(255);
        b.Property(x => x.KtnbLeaderText).HasMaxLength(255);
        b.HasIndex(x => x.PlanId);
        b.HasIndex(x => x.ParentTaskId);
        b.HasIndex(x => x.OwnerDepartmentId);
        b.HasIndex(x => x.ApprovalStatus);
        b.HasOne(x => x.Plan)
            .WithMany(p => p.Tasks)
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.ParentTask)
            .WithMany(p => p.Children)
            .HasForeignKey(x => x.ParentTaskId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.InheritedFromTask)
            .WithMany()
            .HasForeignKey(x => x.InheritedFromTaskId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.AssigneeUser)
            .WithMany()
            .HasForeignKey(x => x.AssigneeUserId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.OwnerDepartment)
            .WithMany()
            .HasForeignKey(x => x.OwnerDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
