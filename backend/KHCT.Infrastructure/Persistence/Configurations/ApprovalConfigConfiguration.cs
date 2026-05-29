using KHCT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KHCT.Infrastructure.Persistence.Configurations;

public class ApprovalConfigConfiguration : IEntityTypeConfiguration<ApprovalConfig>
{
    public void Configure(EntityTypeBuilder<ApprovalConfig> b)
    {
        b.ToTable("approval_config");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.Scope, x.Level }).IsUnique();
        b.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Role)
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
