using KHCT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KHCT.Infrastructure.Persistence.Configurations;

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> b)
    {
        b.ToTable("plan");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.Scope, x.DepartmentId, x.Year, x.Month });
        b.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CreatedBy)
            .WithMany()
            .HasForeignKey(x => x.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.KtnbLeader)
            .WithMany()
            .HasForeignKey(x => x.KtnbLeaderId)
            .OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.RowVersion).IsRowVersion();
    }
}
