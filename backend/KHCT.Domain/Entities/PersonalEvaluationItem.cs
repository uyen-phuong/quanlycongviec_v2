using KHCT.Domain.Common;

namespace KHCT.Domain.Entities;

public class PersonalEvaluationItem : Entity
{
    public Guid PeriodId { get; set; }
    public PersonalEvaluationPeriod? Period { get; set; }
    public int DisplayOrder { get; set; }

    public string? AssignmentSource { get; set; }
    public string? TaskName { get; set; }
    public string? TaskDetail { get; set; }
    public string? ActualResult { get; set; }
    public string? Note { get; set; }

    public DateTime? Deadline { get; set; }
    public DateTime? CompletedAt { get; set; }

    public decimal? SelfProgressScore { get; set; }
    public decimal? SelfQualityScore { get; set; }
    public decimal? TeamLeadProgressScore { get; set; }
    public decimal? TeamLeadQualityScore { get; set; }
    public decimal? ManagerProgressScore { get; set; }
    public decimal? ManagerQualityScore { get; set; }
    public decimal? DeputyProgressScore { get; set; }
    public decimal? DeputyQualityScore { get; set; }
    public decimal? HeadProgressScore { get; set; }
    public decimal? HeadQualityScore { get; set; }
}
