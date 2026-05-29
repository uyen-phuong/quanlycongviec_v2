using KHCT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KHCT.Infrastructure.Persistence.Configurations;

public class LineCommentConfiguration : IEntityTypeConfiguration<LineComment>
{
    public void Configure(EntityTypeBuilder<LineComment> b)
    {
        b.ToTable("line_comment");
        b.HasKey(x => x.Id);
        b.Property(x => x.Content).HasMaxLength(4000).IsRequired();
        b.HasIndex(x => x.TaskId);
        b.HasOne(x => x.Task)
            .WithMany()
            .HasForeignKey(x => x.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.ParentComment)
            .WithMany()
            .HasForeignKey(x => x.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.AuthorUser)
            .WithMany()
            .HasForeignKey(x => x.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ResolvedByUser)
            .WithMany()
            .HasForeignKey(x => x.ResolvedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
