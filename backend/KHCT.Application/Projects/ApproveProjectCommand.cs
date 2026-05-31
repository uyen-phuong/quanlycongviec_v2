using KHCT.Application.Common.Interfaces;
using KHCT.Application.Plans;
using KHCT.Domain.Common;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Projects;

public record ApproveProjectCommand(Guid Id) : IRequest<bool>;

public class ApproveProjectHandler : IRequestHandler<ApproveProjectCommand, bool>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ApproveProjectHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<bool> Handle(ApproveProjectCommand request, CancellationToken ct)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new KeyNotFoundException("Project not found.");

        var isAuthorized = PlanSupport.HasRole(_currentUser, PlanSupport.RoleTruongKtnb) ||
                           PlanSupport.HasRole(_currentUser, PlanSupport.RolePhoTruongKtnb) ||
                           PlanSupport.HasRole(_currentUser, PlanSupport.RoleAdmin);

        if (!isAuthorized)
        {
            throw new ForbiddenException("forbidden_project_approve", "Only KTNB leaders can approve projects.");
        }

        if (project.Status is not WorkflowStatus.Pending)
        {
            throw new DomainException("invalid_status", "Project is not in pending status.");
        }

        project.Status = WorkflowStatus.Approved3;
        project.ApprovedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return true;
    }
}
