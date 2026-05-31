using KHCT.Domain.Common;
using KHCT.Domain.Enums;

namespace KHCT.Domain.Entities;

public class PlanReportingPeriod : Entity
{
    public Guid PlanId { get; set; }
    public Plan? Plan { get; set; }
    
    public string PeriodLabel { get; set; } = string.Empty;
    public string? ProgressText { get; set; }
    public int CompletionPercent { get; set; }
    
    public ReportingPeriodStatus Status { get; set; } = ReportingPeriodStatus.Open;
    
    public Guid? ApprovedByUserId { get; set; }
    public User? ApprovedByUser { get; set; }
    public DateTime? ApprovedAt { get; set; }
}
