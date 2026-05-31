using KHCT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KHCT.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> b)
    {
        b.ToTable("project");
        b.HasKey(x => x.Id);
        
        b.HasOne(x => x.Leader)
            .WithMany()
            .HasForeignKey(x => x.LeaderId)
            .OnDelete(DeleteBehavior.Restrict);
            
        b.HasOne(x => x.SubLeader)
            .WithMany()
            .HasForeignKey(x => x.SubLeaderId)
            .OnDelete(DeleteBehavior.Restrict);
            
        b.Property(x => x.Name).HasMaxLength(500).IsRequired();
        b.Property(x => x.Description).HasMaxLength(2048);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
    }
}
