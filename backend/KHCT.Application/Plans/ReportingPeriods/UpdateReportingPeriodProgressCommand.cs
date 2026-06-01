using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Domain.Common;
using KHCT.Domain.Entities;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Plans.ReportingPeriods;

public record UpdateReportingPeriodProgressCommand(
    Guid PlanId,
    string? ProgressText,
    int CompletionPercent) : IRequest<ReportingPeriodDto>;

public class UpdateReportingPeriodProgressValidator : AbstractValidator<UpdateReportingPeriodProgressCommand>
{
    public UpdateReportingPeriodProgressValidator()
    {
        RuleFor(x => x.CompletionPercent).InclusiveBetween(0, 100);
        RuleFor(x => x.ProgressText).MaximumLength(2048).When(x => x.ProgressText != null);
    }
}

public class UpdateReportingPeriodProgressHandler : IRequestHandler<UpdateReportingPeriodProgressCommand, ReportingPeriodDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public UpdateReportingPeriodProgressHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ReportingPeriodDto> Handle(UpdateReportingPeriodProgressCommand request, CancellationToken ct)
    {
        var plan = await _db.Plans.FirstOrDefaultAsync(x => x.Id == request.PlanId, ct)
            ?? throw new KeyNotFoundException("Plan not found.");

        // Only allow after plan is approved
        if (plan.Status is not (WorkflowStatus.Approved1 or WorkflowStatus.Approved2 or WorkflowStatus.Approved3))
            throw new DomainException("plan_not_approved", "Kế hoạch chưa được phê duyệt, chưa thể cập nhật tiến độ.");

        // Find or create the current open period
        var period = await _db.PlanReportingPeriods
            .FirstOrDefaultAsync(x => x.PlanId == plan.Id && x.Status == ReportingPeriodStatus.Open, ct);

        if (period is null)
        {
            period = new PlanReportingPeriod
            {
                Id = Guid.NewGuid(),
                PlanId = plan.Id,
                PeriodLabel = GeneratePeriodLabel(plan),
                Status = ReportingPeriodStatus.Open,
                CompletionPercent = 0
            };
            _db.PlanReportingPeriods.Add(period);
        }

        period.ProgressText = string.IsNullOrWhiteSpace(request.ProgressText) ? null : request.ProgressText.Trim();
        period.CompletionPercent = request.CompletionPercent;

        await _db.SaveChangesAsync(ct);

        return new ReportingPeriodDto(
            period.Id, period.PlanId, period.PeriodLabel,
            period.ProgressText, period.CompletionPercent,
            period.Status.ToString().ToLowerInvariant(),
            null, null, null,
            period.CreatedAt, period.UpdatedAt);
    }

    private static string GeneratePeriodLabel(Domain.Entities.Plan plan)
    {
        return plan.ReportingPeriodType switch
        {
            ReportingPeriodType.Monthly => $"Tháng {plan.Month:00}/{plan.Year}",
            ReportingPeriodType.Quarterly => $"Quý {(plan.Month - 1) / 3 + 1}/{plan.Year}",
            ReportingPeriodType.SemiAnnual => plan.Month <= 6 ? $"6 tháng đầu {plan.Year}" : $"6 tháng cuối {plan.Year}",
            ReportingPeriodType.Annual => $"Năm {plan.Year}",
            _ => $"Kỳ {plan.CurrentPeriodIndex + 1}/{plan.Year}"
        };
    }
}
