using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.PersonalEvaluations.Commands;

public record UpdatePersonalEvaluationItemCommand(
    Guid Id,
    int DisplayOrder,
    string? AssignmentSource,
    string? TaskName,
    string? TaskDetail,
    string? ActualResult,
    string? Note,
    DateTime? Deadline,
    DateTime? CompletedAt,
    decimal? SelfProgressScore,
    decimal? SelfQualityScore,
    decimal? TeamLeadProgressScore,
    decimal? TeamLeadQualityScore,
    decimal? ManagerProgressScore,
    decimal? ManagerQualityScore,
    decimal? DeputyProgressScore,
    decimal? DeputyQualityScore,
    decimal? HeadProgressScore,
    decimal? HeadQualityScore) : IRequest<PersonalEvaluationItemDto>;

public class UpdatePersonalEvaluationItemValidator : AbstractValidator<UpdatePersonalEvaluationItemCommand>
{
    public UpdatePersonalEvaluationItemValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AssignmentSource).MaximumLength(500);
        RuleFor(x => x.TaskName).MaximumLength(500);
        foreach (var sel in new System.Linq.Expressions.Expression<Func<UpdatePersonalEvaluationItemCommand, decimal?>>[]
        {
            x => x.SelfProgressScore, x => x.SelfQualityScore,
            x => x.TeamLeadProgressScore, x => x.TeamLeadQualityScore,
            x => x.ManagerProgressScore, x => x.ManagerQualityScore,
            x => x.DeputyProgressScore, x => x.DeputyQualityScore,
            x => x.HeadProgressScore, x => x.HeadQualityScore
        })
        {
            RuleFor(sel).InclusiveBetween(0m, 20m).When(x => sel.Compile().Invoke(x).HasValue);
        }
    }
}

public class UpdatePersonalEvaluationItemHandler : IRequestHandler<UpdatePersonalEvaluationItemCommand, PersonalEvaluationItemDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public UpdatePersonalEvaluationItemHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async System.Threading.Tasks.Task<PersonalEvaluationItemDto> Handle(UpdatePersonalEvaluationItemCommand request, CancellationToken ct)
    {
        var item = await _db.PersonalEvaluationItems
            .Include(x => x.Period)
                .ThenInclude(p => p!.User)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new KeyNotFoundException("item not found");

        var period = item.Period ?? throw new InvalidOperationException("period missing");
        var user = period.User ?? throw new InvalidOperationException("user missing");

        var canText = PersonalEvaluationSupport.CanEditItemText(_currentUser, user);
        var canSelf = PersonalEvaluationSupport.CanScoreColumn(_currentUser, user, Scorer.Self);
        var canTeam = PersonalEvaluationSupport.CanScoreColumn(_currentUser, user, Scorer.TeamLead);
        var canMgr = PersonalEvaluationSupport.CanScoreColumn(_currentUser, user, Scorer.Manager);
        var canDeputy = PersonalEvaluationSupport.CanScoreColumn(_currentUser, user, Scorer.Deputy);
        var canHead = PersonalEvaluationSupport.CanScoreColumn(_currentUser, user, Scorer.Head);

        EnsureNoChange(canText, request.DisplayOrder, item.DisplayOrder, "display_order");
        EnsureNoChange(canText, Norm(request.AssignmentSource), item.AssignmentSource);
        EnsureNoChange(canText, Norm(request.TaskName), item.TaskName);
        EnsureNoChange(canText, Norm(request.TaskDetail), item.TaskDetail);
        EnsureNoChange(canText, Norm(request.ActualResult), item.ActualResult);
        EnsureNoChange(canText, Norm(request.Note), item.Note);
        EnsureNoChange(canText, request.Deadline, item.Deadline);
        EnsureNoChange(canText, request.CompletedAt, item.CompletedAt);

        EnsureNoChange(canSelf, request.SelfProgressScore, item.SelfProgressScore);
        EnsureNoChange(canSelf, request.SelfQualityScore, item.SelfQualityScore);
        EnsureNoChange(canTeam, request.TeamLeadProgressScore, item.TeamLeadProgressScore);
        EnsureNoChange(canTeam, request.TeamLeadQualityScore, item.TeamLeadQualityScore);
        EnsureNoChange(canMgr, request.ManagerProgressScore, item.ManagerProgressScore);
        EnsureNoChange(canMgr, request.ManagerQualityScore, item.ManagerQualityScore);
        EnsureNoChange(canDeputy, request.DeputyProgressScore, item.DeputyProgressScore);
        EnsureNoChange(canDeputy, request.DeputyQualityScore, item.DeputyQualityScore);
        EnsureNoChange(canHead, request.HeadProgressScore, item.HeadProgressScore);
        EnsureNoChange(canHead, request.HeadQualityScore, item.HeadQualityScore);

        if (canText)
        {
            item.DisplayOrder = request.DisplayOrder;
            item.AssignmentSource = Norm(request.AssignmentSource);
            item.TaskName = Norm(request.TaskName);
            item.TaskDetail = Norm(request.TaskDetail);
            item.ActualResult = Norm(request.ActualResult);
            item.Note = Norm(request.Note);
            item.Deadline = request.Deadline;
            item.CompletedAt = request.CompletedAt;
        }
        if (canSelf) { item.SelfProgressScore = request.SelfProgressScore; item.SelfQualityScore = request.SelfQualityScore; }
        if (canTeam) { item.TeamLeadProgressScore = request.TeamLeadProgressScore; item.TeamLeadQualityScore = request.TeamLeadQualityScore; }
        if (canMgr) { item.ManagerProgressScore = request.ManagerProgressScore; item.ManagerQualityScore = request.ManagerQualityScore; }
        if (canDeputy) { item.DeputyProgressScore = request.DeputyProgressScore; item.DeputyQualityScore = request.DeputyQualityScore; }
        if (canHead) { item.HeadProgressScore = request.HeadProgressScore; item.HeadQualityScore = request.HeadQualityScore; }

        await _db.SaveChangesAsync(ct);
        return PersonalEvaluationSupport.ToDto(item);
    }

    private static string? Norm(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

    private static void EnsureNoChange<T>(bool allowed, T proposed, T current, string? fieldName = null)
    {
        if (allowed) return;
        if (!Equals(proposed, current))
            throw new ForbiddenException(PersonalEvaluationSupport.ForbiddenFieldChange, "Không có quyền sửa trường này.");
    }
}
