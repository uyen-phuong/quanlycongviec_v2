using KHCT.Application.Common.Interfaces;
using KHCT.Domain.Common;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Plans.ReportingPeriods;

public record ApproveReportingPeriodCommand(Guid PlanId) : IRequest<ReportingPeriodDto>;

public class ApproveReportingPeriodHandler : IRequestHandler<ApproveReportingPeriodCommand, ReportingPeriodDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ApproveReportingPeriodHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ReportingPeriodDto> Handle(ApproveReportingPeriodCommand request, CancellationToken ct)
    {
        // Only KTNB leader or Admin can approve periods
        if (!PlanSupport.HasRole(_currentUser, PlanSupport.RoleTruongKtnb) &&
            !PlanSupport.HasRole(_currentUser, PlanSupport.RolePhoTruongKtnb) &&
            !PlanSupport.HasRole(_currentUser, PlanSupport.RoleAdmin))
        {
            throw new ForbiddenException("forbidden_role", "Chỉ Lãnh đạo KTNB mới có thể duyệt kỳ báo cáo.");
        }

        var plan = await _db.Plans.FirstOrDefaultAsync(x => x.Id == request.PlanId, ct)
            ?? throw new KeyNotFoundException("Plan not found.");

        // Find the current open period
        var period = await _db.PlanReportingPeriods
            .FirstOrDefaultAsync(x => x.PlanId == plan.Id && x.Status == ReportingPeriodStatus.Open, ct)
            ?? throw new DomainException("no_open_period", "Không có kỳ báo cáo nào đang mở để phê duyệt.");

        // Close the current period
        var now = DateTime.UtcNow;
        period.Status = ReportingPeriodStatus.Closed;
        period.ApprovedByUserId = _currentUser.UserId;
        period.ApprovedAt = now;

        // Advance period index
        plan.CurrentPeriodIndex++;

        // If annual report (1 period total), mark plan as completed
        if (plan.ReportingPeriodType == ReportingPeriodType.Annual)
        {
            plan.Status = WorkflowStatus.Approved3;
        }

        await _db.SaveChangesAsync(ct);

        return new ReportingPeriodDto(
            period.Id, period.PlanId, period.PeriodLabel,
            period.ProgressText, period.CompletionPercent,
            period.Status.ToString().ToLowerInvariant(),
            period.ApprovedByUserId,
            null,
            period.ApprovedAt,
            period.CreatedAt, period.UpdatedAt);
    }
}
