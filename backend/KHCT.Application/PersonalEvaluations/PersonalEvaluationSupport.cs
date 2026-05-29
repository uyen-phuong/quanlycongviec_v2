using KHCT.Application.Common.Interfaces;
using KHCT.Domain.Entities;

namespace KHCT.Application.PersonalEvaluations;

public enum Scorer
{
    None,
    Self,
    TeamLead,
    Manager,
    Deputy,
    Head
}

public static class PersonalEvaluationSupport
{
    public const string ForbiddenFieldChange = "forbidden_field_change";
    public const string ForbiddenRole = "forbidden_role";

    public static Scorer GetScorerForRole(IReadOnlyCollection<string> roles)
    {
        if (roles.Contains("TRUONG_KTNB")) return Scorer.Head;
        if (roles.Contains("PHO_TRUONG_KTNB")) return Scorer.Deputy;
        if (roles.Contains("TRUONG_PHONG")) return Scorer.Manager;
        if (roles.Contains("TRUONG_NHOM")) return Scorer.TeamLead;
        if (roles.Contains("NHAN_VIEN")) return Scorer.Self;
        return Scorer.None;
    }

    public static bool CanReadPeriodOf(ICurrentUser current, User targetUser)
    {
        var roles = current.Roles;
        if (roles.Contains("ADMIN") || roles.Contains("TRUONG_KTNB") || roles.Contains("PHO_TRUONG_KTNB") || roles.Contains("TRUONG_KH") || roles.Contains("VAN_THU"))
            return true;
        if (roles.Contains("TRUONG_PHONG") || roles.Contains("TRUONG_NHOM"))
            return current.DepartmentId.HasValue && targetUser.DepartmentId == current.DepartmentId;
        return current.UserId == targetUser.Id;
    }

    public static bool CanScoreColumn(ICurrentUser current, User targetUser, Scorer column)
    {
        if (column == Scorer.None) return false;
        var myScorer = GetScorerForRole(current.Roles);
        if (myScorer != column) return false;

        return column switch
        {
            Scorer.Self => current.UserId == targetUser.Id,
            Scorer.TeamLead => current.DepartmentId.HasValue && current.DepartmentId == targetUser.DepartmentId,
            Scorer.Manager => current.DepartmentId.HasValue && current.DepartmentId == targetUser.DepartmentId,
            Scorer.Deputy => true,
            Scorer.Head => true,
            _ => false
        };
    }

    public static bool CanEditItemText(ICurrentUser current, User targetUser) =>
        current.UserId == targetUser.Id && current.Roles.Contains("NHAN_VIEN");

    public static bool CanCreateOrDeleteItem(ICurrentUser current, User targetUser) =>
        current.UserId == targetUser.Id && current.Roles.Contains("NHAN_VIEN");

    public static PersonalEvaluationItemDto ToDto(PersonalEvaluationItem item) =>
        new(
            item.Id,
            item.PeriodId,
            item.DisplayOrder,
            item.AssignmentSource,
            item.TaskName,
            item.TaskDetail,
            item.ActualResult,
            item.Note,
            item.Deadline,
            item.CompletedAt,
            item.SelfProgressScore,
            item.SelfQualityScore,
            item.TeamLeadProgressScore,
            item.TeamLeadQualityScore,
            item.ManagerProgressScore,
            item.ManagerQualityScore,
            item.DeputyProgressScore,
            item.DeputyQualityScore,
            item.HeadProgressScore,
            item.HeadQualityScore);

    public static PersonalEvaluationPeriodDto ToDto(PersonalEvaluationPeriod period) =>
        new(
            period.Id,
            period.UserId,
            period.User?.FullName ?? string.Empty,
            period.DepartmentId,
            period.Department?.Name ?? string.Empty,
            period.ReportYear,
            period.ReportMonth,
            period.Status.ToString(),
            period.CapacityAttitudeSelfScore,
            period.CapacityAttitudeTeamLeadScore,
            period.CapacityAttitudeManagerScore,
            period.CapacityAttitudeDeputyScore,
            period.CapacityAttitudeHeadScore,
            period.DisciplineSelfScore,
            period.DisciplineTeamLeadScore,
            period.DisciplineManagerScore,
            period.DisciplineDeputyScore,
            period.DisciplineHeadScore,
            period.InspectionSelfScore,
            period.InspectionTeamLeadScore,
            period.InspectionManagerScore,
            period.InspectionDeputyScore,
            period.InspectionHeadScore);
}
