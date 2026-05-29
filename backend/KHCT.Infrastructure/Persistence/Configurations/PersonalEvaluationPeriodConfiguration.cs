using KHCT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KHCT.Infrastructure.Persistence.Configurations;

public class PersonalEvaluationPeriodConfiguration : IEntityTypeConfiguration<PersonalEvaluationPeriod>
{
    public void Configure(EntityTypeBuilder<PersonalEvaluationPeriod> b)
    {
        b.ToTable("personal_evaluation_period");
        b.HasKey(x => x.Id);

        b.HasIndex(x => new { x.UserId, x.ReportYear, x.ReportMonth }).IsUnique();
        b.HasIndex(x => new { x.DepartmentId, x.ReportYear, x.ReportMonth });

        b.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        foreach (var prop in new[]
        {
            nameof(PersonalEvaluationPeriod.CapacityAttitudeSelfScore),
            nameof(PersonalEvaluationPeriod.CapacityAttitudeTeamLeadScore),
            nameof(PersonalEvaluationPeriod.CapacityAttitudeManagerScore),
            nameof(PersonalEvaluationPeriod.CapacityAttitudeDeputyScore),
            nameof(PersonalEvaluationPeriod.CapacityAttitudeHeadScore),
            nameof(PersonalEvaluationPeriod.DisciplineSelfScore),
            nameof(PersonalEvaluationPeriod.DisciplineTeamLeadScore),
            nameof(PersonalEvaluationPeriod.DisciplineManagerScore),
            nameof(PersonalEvaluationPeriod.DisciplineDeputyScore),
            nameof(PersonalEvaluationPeriod.DisciplineHeadScore),
            nameof(PersonalEvaluationPeriod.InspectionSelfScore),
            nameof(PersonalEvaluationPeriod.InspectionTeamLeadScore),
            nameof(PersonalEvaluationPeriod.InspectionManagerScore),
            nameof(PersonalEvaluationPeriod.InspectionDeputyScore),
            nameof(PersonalEvaluationPeriod.InspectionHeadScore)
        })
        {
            b.Property<decimal?>(prop).HasColumnType("decimal(4,1)");
        }
    }
}
