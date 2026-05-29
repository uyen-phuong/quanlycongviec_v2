using KHCT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KHCT.Infrastructure.Persistence.Configurations;

public class TaskSupportingDeptConfiguration : IEntityTypeConfiguration<TaskSupportingDept>
{
    public void Configure(EntityTypeBuilder<TaskSupportingDept> b)
    {
        b.ToTable("task_supporting_dept");
        b.HasKey(x => new { x.TaskId, x.DepartmentId });
        b.HasOne(x => x.Task)
            .WithMany(t => t.SupportingDepts)
            .HasForeignKey(x => x.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
