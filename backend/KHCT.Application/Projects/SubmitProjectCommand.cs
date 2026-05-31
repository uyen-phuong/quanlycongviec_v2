using KHCT.Application.Common.Interfaces;
using KHCT.Application.Plans;
using KHCT.Domain.Common;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Projects;

public record SubmitProjectCommand(Guid Id) : IRequest<bool>;

public class SubmitProjectHandler : IRequestHandler<SubmitProjectCommand, bool>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public SubmitProjectHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<bool> Handle(SubmitProjectCommand request, CancellationToken ct)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new KeyNotFoundException("Project not found.");

        var isAuthorized = PlanSupport.HasRole(_currentUser, PlanSupport.RoleVanThu) ||
                           PlanSupport.HasRole(_currentUser, PlanSupport.RoleAdmin);

        if (!isAuthorized)
        {
            throw new ForbiddenException("forbidden_project_submit", "Only Van Thu or Admin can submit projects.");
        }

        if (project.Status is not (WorkflowStatus.Draft or WorkflowStatus.Returned))
        {
            throw new DomainException("invalid_status", "Project is not in draft or returned status.");
        }

        project.Status = WorkflowStatus.Pending;
        project.SubmittedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return true;
    }
}
