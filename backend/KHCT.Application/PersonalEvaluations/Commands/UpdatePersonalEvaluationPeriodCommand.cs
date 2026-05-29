using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.PersonalEvaluations.Commands;

public record UpdatePersonalEvaluationPeriodCommand(
    Guid Id,
    decimal? CapacityAttitudeSelfScore,
    decimal? CapacityAttitudeTeamLeadScore,
    decimal? CapacityAttitudeManagerScore,
    decimal? CapacityAttitudeDeputyScore,
    decimal? CapacityAttitudeHeadScore,
    decimal? DisciplineSelfScore,
    decimal? DisciplineTeamLeadScore,
    decimal? DisciplineManagerScore,
    decimal? DisciplineDeputyScore,
    decimal? DisciplineHeadScore,
    decimal? InspectionSelfScore,
    decimal? InspectionTeamLeadScore,
    decimal? InspectionManagerScore,
    decimal? InspectionDeputyScore,
    decimal? InspectionHeadScore) : IRequest<PersonalEvaluationPeriodDto>;

public class UpdatePersonalEvaluationPeriodValidator : AbstractValidator<UpdatePersonalEvaluationPeriodCommand>
{
    public UpdatePersonalEvaluationPeriodValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.CapacityAttitudeSelfScore).InclusiveBetween(0m, 20m).When(x => x.CapacityAttitudeSelfScore.HasValue);
        RuleFor(x => x.CapacityAttitudeTeamLeadScore).InclusiveBetween(0m, 20m).When(x => x.CapacityAttitudeTeamLeadScore.HasValue);
        RuleFor(x => x.CapacityAttitudeManagerScore).InclusiveBetween(0m, 20m).When(x => x.CapacityAttitudeManagerScore.HasValue);
        RuleFor(x => x.CapacityAttitudeDeputyScore).InclusiveBetween(0m, 20m).When(x => x.CapacityAttitudeDeputyScore.HasValue);
        RuleFor(x => x.CapacityAttitudeHeadScore).InclusiveBetween(0m, 20m).When(x => x.CapacityAttitudeHeadScore.HasValue);
        RuleFor(x => x.DisciplineSelfScore).InclusiveBetween(0m, 10m).When(x => x.DisciplineSelfScore.HasValue);
        RuleFor(x => x.DisciplineTeamLeadScore).InclusiveBetween(0m, 10m).When(x => x.DisciplineTeamLeadScore.HasValue);
        RuleFor(x => x.DisciplineManagerScore).InclusiveBetween(0m, 10m).When(x => x.DisciplineManagerScore.HasValue);
        RuleFor(x => x.DisciplineDeputyScore).InclusiveBetween(0m, 10m).When(x => x.DisciplineDeputyScore.HasValue);
        RuleFor(x => x.DisciplineHeadScore).InclusiveBetween(0m, 10m).When(x => x.DisciplineHeadScore.HasValue);
        RuleFor(x => x.InspectionSelfScore).InclusiveBetween(0m, 10m).When(x => x.InspectionSelfScore.HasValue);
        RuleFor(x => x.InspectionTeamLeadScore).InclusiveBetween(0m, 10m).When(x => x.InspectionTeamLeadScore.HasValue);
        RuleFor(x => x.InspectionManagerScore).InclusiveBetween(0m, 10m).When(x => x.InspectionManagerScore.HasValue);
        RuleFor(x => x.InspectionDeputyScore).InclusiveBetween(0m, 10m).When(x => x.InspectionDeputyScore.HasValue);
        RuleFor(x => x.InspectionHeadScore).InclusiveBetween(0m, 10m).When(x => x.InspectionHeadScore.HasValue);
    }
}

public class UpdatePersonalEvaluationPeriodHandler : IRequestHandler<UpdatePersonalEvaluationPeriodCommand, PersonalEvaluationPeriodDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public UpdatePersonalEvaluationPeriodHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async System.Threading.Tasks.Task<PersonalEvaluationPeriodDto> Handle(UpdatePersonalEvaluationPeriodCommand request, CancellationToken ct)
    {
        var period = await _db.PersonalEvaluationPeriods
            .Include(x => x.User)
            .Include(x => x.Department)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new KeyNotFoundException("period not found");

        var user = period.User ?? throw new InvalidOperationException();
        var canSelf = PersonalEvaluationSupport.CanScoreColumn(_currentUser, user, Scorer.Self);
        var canTeam = PersonalEvaluationSupport.CanScoreColumn(_currentUser, user, Scorer.TeamLead);
        var canMgr = PersonalEvaluationSupport.CanScoreColumn(_currentUser, user, Scorer.Manager);
        var canDeputy = PersonalEvaluationSupport.CanScoreColumn(_currentUser, user, Scorer.Deputy);
        var canHead = PersonalEvaluationSupport.CanScoreColumn(_currentUser, user, Scorer.Head);

        EnsureNoChange(canSelf, request.CapacityAttitudeSelfScore, period.CapacityAttitudeSelfScore);
        EnsureNoChange(canSelf, request.DisciplineSelfScore, period.DisciplineSelfScore);
        EnsureNoChange(canSelf, request.InspectionSelfScore, period.InspectionSelfScore);
        EnsureNoChange(canTeam, request.CapacityAttitudeTeamLeadScore, period.CapacityAttitudeTeamLeadScore);
        EnsureNoChange(canTeam, request.DisciplineTeamLeadScore, period.DisciplineTeamLeadScore);
        EnsureNoChange(canTeam, request.InspectionTeamLeadScore, period.InspectionTeamLeadScore);
        EnsureNoChange(canMgr, request.CapacityAttitudeManagerScore, period.CapacityAttitudeManagerScore);
        EnsureNoChange(canMgr, request.DisciplineManagerScore, period.DisciplineManagerScore);
        EnsureNoChange(canMgr, request.InspectionManagerScore, period.InspectionManagerScore);
        EnsureNoChange(canDeputy, request.CapacityAttitudeDeputyScore, period.CapacityAttitudeDeputyScore);
        EnsureNoChange(canDeputy, request.DisciplineDeputyScore, period.DisciplineDeputyScore);
        EnsureNoChange(canDeputy, request.InspectionDeputyScore, period.InspectionDeputyScore);
        EnsureNoChange(canHead, request.CapacityAttitudeHeadScore, period.CapacityAttitudeHeadScore);
        EnsureNoChange(canHead, request.DisciplineHeadScore, period.DisciplineHeadScore);
        EnsureNoChange(canHead, request.InspectionHeadScore, period.InspectionHeadScore);

        if (canSelf) { period.CapacityAttitudeSelfScore = request.CapacityAttitudeSelfScore; period.DisciplineSelfScore = request.DisciplineSelfScore; period.InspectionSelfScore = request.InspectionSelfScore; }
        if (canTeam) { period.CapacityAttitudeTeamLeadScore = request.CapacityAttitudeTeamLeadScore; period.DisciplineTeamLeadScore = request.DisciplineTeamLeadScore; period.InspectionTeamLeadScore = request.InspectionTeamLeadScore; }
        if (canMgr) { period.CapacityAttitudeManagerScore = request.CapacityAttitudeManagerScore; period.DisciplineManagerScore = request.DisciplineManagerScore; period.InspectionManagerScore = request.InspectionManagerScore; }
        if (canDeputy) { period.CapacityAttitudeDeputyScore = request.CapacityAttitudeDeputyScore; period.DisciplineDeputyScore = request.DisciplineDeputyScore; period.InspectionDeputyScore = request.InspectionDeputyScore; }
        if (canHead) { period.CapacityAttitudeHeadScore = request.CapacityAttitudeHeadScore; period.DisciplineHeadScore = request.DisciplineHeadScore; period.InspectionHeadScore = request.InspectionHeadScore; }

        await _db.SaveChangesAsync(ct);
        return PersonalEvaluationSupport.ToDto(period);
    }

    private static void EnsureNoChange(bool allowed, decimal? proposed, decimal? current)
    {
        if (allowed) return;
        if (proposed != current) throw new ForbiddenException(PersonalEvaluationSupport.ForbiddenFieldChange, "Không có quyền sửa trường này.");
    }
}
