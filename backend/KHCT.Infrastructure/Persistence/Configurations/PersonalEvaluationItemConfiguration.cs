using KHCT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KHCT.Infrastructure.Persistence.Configurations;

public class PersonalEvaluationItemConfiguration : IEntityTypeConfiguration<PersonalEvaluationItem>
{
    public void Configure(EntityTypeBuilder<PersonalEvaluationItem> b)
    {
        b.ToTable("personal_evaluation_item");
        b.HasKey(x => x.Id);

        b.Property(x => x.AssignmentSource).HasMaxLength(500);
        b.Property(x => x.TaskName).HasMaxLength(500);

        b.HasIndex(x => new { x.PeriodId, x.DisplayOrder });

        b.HasOne(x => x.Period)
            .WithMany(p => p.Items)
            .HasForeignKey(x => x.PeriodId)
            .OnDelete(DeleteBehavior.Cascade);

        foreach (var prop in new[]
        {
            nameof(PersonalEvaluationItem.SelfProgressScore),
            nameof(PersonalEvaluationItem.SelfQualityScore),
            nameof(PersonalEvaluationItem.TeamLeadProgressScore),
            nameof(PersonalEvaluationItem.TeamLeadQualityScore),
            nameof(PersonalEvaluationItem.ManagerProgressScore),
            nameof(PersonalEvaluationItem.ManagerQualityScore),
            nameof(PersonalEvaluationItem.DeputyProgressScore),
            nameof(PersonalEvaluationItem.DeputyQualityScore),
            nameof(PersonalEvaluationItem.HeadProgressScore),
            nameof(PersonalEvaluationItem.HeadQualityScore)
        })
        {
            b.Property<decimal?>(prop).HasColumnType("decimal(4,1)");
        }
    }
}
