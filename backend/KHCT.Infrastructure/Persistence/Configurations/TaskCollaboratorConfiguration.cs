using KHCT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KHCT.Infrastructure.Persistence.Configurations;

public class TaskCollaboratorConfiguration : IEntityTypeConfiguration<TaskCollaborator>
{
    public void Configure(EntityTypeBuilder<TaskCollaborator> b)
    {
        b.ToTable("task_collaborator");
        b.HasKey(x => new { x.TaskId, x.UserId });

        b.HasOne(x => x.Task)
            .WithMany(t => t.Collaborators)
            .HasForeignKey(x => x.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Property(x => x.CollaborationContent).HasMaxLength(2048);
    }
}
