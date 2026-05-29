using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Application.Common.Support;
using KHCT.Domain.Common;
using KHCT.Domain.Entities;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Plans.Workflow;

public record CreateLineCommentCommand(Guid TaskId, string Content) : IRequest<LineCommentDto>;

public class CreateLineCommentCommandValidator : AbstractValidator<CreateLineCommentCommand>
{
    public CreateLineCommentCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
        RuleFor(x => x.Content).NotEmpty().MaximumLength(4000);
    }
}

public class CreateLineCommentHandler : IRequestHandler<CreateLineCommentCommand, LineCommentDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CreateLineCommentHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<LineCommentDto> Handle(CreateLineCommentCommand request, CancellationToken ct)
    {
        var task = await _db.Tasks
            .Include(x => x.Plan)
                .ThenInclude(x => x!.Department)
            .FirstOrDefaultAsync(x => x.Id == request.TaskId, ct)
            ?? throw new KeyNotFoundException("Task not found.");

        var plan = task.Plan ?? throw new KeyNotFoundException("Plan not found.");

        var authorRole = WorkflowSupport.ResolveCommentRole(_currentUser);
        if (authorRole == CommentRole.Creator)
        {
            throw new ForbiddenException("forbidden_role", "Current role cannot add line comments.");
        }

        var actorId = PlanSupport.RequireActorId(_currentUser);
        var comment = new LineComment
        {
            Id = Guid.NewGuid(),
            TaskId = task.Id,
            AuthorUserId = actorId,
            AuthorRole = authorRole,
            Content = request.Content.Trim(),
            IsResolved = false
        };
        _db.LineComments.Add(comment);
        task.HasOpenComment = true;

        _db.AuditLogs.Add(ApplicationSupport.CreateAudit(
            "line_comment",
            comment.Id,
            "create",
            _currentUser.UserId,
            null,
            new
            {
                TaskId = task.Id,
                TaskTitle = task.Title,
                Content = comment.Content,
                AuthorRole = WorkflowSupport.CommentRoleCode(authorRole),
                PlanId = plan.Id
            }));

        await _db.SaveChangesAsync(ct);

        var result = await _db.LineComments
            .AsNoTracking()
            .Include(x => x.Task)
            .Include(x => x.AuthorUser)
            .Include(x => x.ResolvedByUser)
            .FirstAsync(x => x.Id == comment.Id, ct);

        return WorkflowSupport.ToDto(result);
    }
}
