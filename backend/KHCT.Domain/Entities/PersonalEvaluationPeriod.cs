using KHCT.Domain.Common;
using KHCT.Domain.Enums;

namespace KHCT.Domain.Entities;

public class PersonalEvaluationPeriod : Entity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public Guid DepartmentId { get; set; }
    public Department? Department { get; set; }
    public int ReportYear { get; set; }
    public int ReportMonth { get; set; }

    public decimal? CapacityAttitudeSelfScore { get; set; }
    public decimal? CapacityAttitudeTeamLeadScore { get; set; }
    public decimal? CapacityAttitudeManagerScore { get; set; }
    public decimal? CapacityAttitudeDeputyScore { get; set; }
    public decimal? CapacityAttitudeHeadScore { get; set; }

    public decimal? DisciplineSelfScore { get; set; }
    public decimal? DisciplineTeamLeadScore { get; set; }
    public decimal? DisciplineManagerScore { get; set; }
    public decimal? DisciplineDeputyScore { get; set; }
    public decimal? DisciplineHeadScore { get; set; }

    public decimal? InspectionSelfScore { get; set; }
    public decimal? InspectionTeamLeadScore { get; set; }
    public decimal? InspectionManagerScore { get; set; }
    public decimal? InspectionDeputyScore { get; set; }
    public decimal? InspectionHeadScore { get; set; }

    public PersonalEvaluationPeriodStatus Status { get; set; } = PersonalEvaluationPeriodStatus.Draft;

    public ICollection<PersonalEvaluationItem> Items { get; set; } = new List<PersonalEvaluationItem>();
}
