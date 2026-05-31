using KHCT.Application.Common.Interfaces;
using KHCT.Application.Plans;
using KHCT.Domain.Common;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Projects;

public record ReturnProjectCommand(Guid Id, string Comment) : IRequest<bool>;

public class ReturnProjectHandler : IRequestHandler<ReturnProjectCommand, bool>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ReturnProjectHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<bool> Handle(ReturnProjectCommand request, CancellationToken ct)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new KeyNotFoundException("Project not found.");

        var isAuthorized = PlanSupport.HasRole(_currentUser, PlanSupport.RoleTruongKtnb) ||
                           PlanSupport.HasRole(_currentUser, PlanSupport.RolePhoTruongKtnb) ||
                           PlanSupport.HasRole(_currentUser, PlanSupport.RoleTruongPhong) || // TP of Văn thư or TP of Owner dept
                           PlanSupport.HasRole(_currentUser, PlanSupport.RoleAdmin);

        if (!isAuthorized)
        {
            throw new ForbiddenException("forbidden_project_return", "Only leaders or authorized roles can return projects.");
        }

        if (project.Status is not WorkflowStatus.Pending)
        {
            throw new DomainException("invalid_status", "Project is not in pending status.");
        }

        project.Status = WorkflowStatus.Returned;
        project.SubmittedAt = null;

        await _db.SaveChangesAsync(ct);
        return true;
    }
}
