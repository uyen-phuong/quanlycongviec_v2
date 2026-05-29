using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Application.Common.Support;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Plans.Workflow;

public record ResolveLineCommentCommand(Guid CommentId) : IRequest<LineCommentDto>;

public class ResolveLineCommentCommandValidator : AbstractValidator<ResolveLineCommentCommand>
{
    public ResolveLineCommentCommandValidator()
    {
        RuleFor(x => x.CommentId).NotEmpty();
    }
}

public class ResolveLineCommentHandler : IRequestHandler<ResolveLineCommentCommand, LineCommentDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ResolveLineCommentHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<LineCommentDto> Handle(ResolveLineCommentCommand request, CancellationToken ct)
    {
        var comment = await _db.LineComments
            .Include(x => x.Task)
                .ThenInclude(x => x!.Plan)
            .Include(x => x.AuthorUser)
            .Include(x => x.ResolvedByUser)
            .FirstOrDefaultAsync(x => x.Id == request.CommentId, ct)
            ?? throw new KeyNotFoundException("Line comment not found.");

        var plan = comment.Task?.Plan ?? throw new KeyNotFoundException("Plan not found.");
        WorkflowSupport.EnsureCanResolveComment(plan, _currentUser);

        if (!comment.IsResolved)
        {
            comment.IsResolved = true;
            comment.ResolvedAt = DateTime.UtcNow;
            comment.ResolvedByUserId = PlanSupport.RequireActorId(_currentUser);

            var stillOpen = await _db.LineComments.AnyAsync(x => x.TaskId == comment.TaskId && !x.IsResolved && x.Id != comment.Id, ct);
            if (!stillOpen && comment.Task != null)
            {
                comment.Task.HasOpenComment = false;
            }

            _db.AuditLogs.Add(ApplicationSupport.CreateAudit(
                "line_comment",
                comment.Id,
                "resolve",
                _currentUser.UserId,
                new { IsResolved = false },
                new { IsResolved = true, ResolvedAt = comment.ResolvedAt }));

            await _db.SaveChangesAsync(ct);
        }

        var result = await _db.LineComments
            .AsNoTracking()
            .Include(x => x.Task)
            .Include(x => x.AuthorUser)
            .Include(x => x.ResolvedByUser)
            .FirstAsync(x => x.Id == comment.Id, ct);

        return WorkflowSupport.ToDto(result);
    }
}
