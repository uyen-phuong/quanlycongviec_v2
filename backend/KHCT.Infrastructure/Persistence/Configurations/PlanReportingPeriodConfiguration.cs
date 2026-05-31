using KHCT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KHCT.Infrastructure.Persistence.Configurations;

public class PlanReportingPeriodConfiguration : IEntityTypeConfiguration<PlanReportingPeriod>
{
    public void Configure(EntityTypeBuilder<PlanReportingPeriod> b)
    {
        b.ToTable("plan_reporting_period");
        b.HasKey(x => x.Id);
        
        b.HasOne(x => x.Plan)
            .WithMany()
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Cascade);
            
        b.HasOne(x => x.ApprovedByUser)
            .WithMany()
            .HasForeignKey(x => x.ApprovedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
            
        b.Property(x => x.PeriodLabel).HasMaxLength(128).IsRequired();
        b.Property(x => x.ProgressText).HasMaxLength(2048);
    }
}
