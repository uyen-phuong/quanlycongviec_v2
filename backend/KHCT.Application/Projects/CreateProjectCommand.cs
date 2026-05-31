using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Application.Plans;
using KHCT.Domain.Common;
using KHCT.Domain.Entities;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Projects;

public record CreateProjectCommand(
    string Name,
    string Description,
    Guid LeaderId,
    Guid? SubLeaderId,
    IReadOnlyList<Guid> MemberUserIds) : IRequest<ProjectDetailDto>;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Description).MaximumLength(2048);
        RuleFor(x => x.LeaderId).NotEmpty();
        RuleFor(x => x.MemberUserIds).NotNull();
    }
}

public class CreateProjectHandler : IRequestHandler<CreateProjectCommand, ProjectDetailDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CreateProjectHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ProjectDetailDto> Handle(CreateProjectCommand request, CancellationToken ct)
    {
        // Section 5 PRD says VanThu or Admin can create projects
        var isAuthorized = PlanSupport.HasRole(_currentUser, PlanSupport.RoleVanThu) ||
                           PlanSupport.HasRole(_currentUser, PlanSupport.RoleAdmin);

        if (!isAuthorized)
        {
            throw new ForbiddenException("forbidden_project_creation", "Only Van Thu or Admin can create new projects.");
        }

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            LeaderId = request.LeaderId,
            SubLeaderId = request.SubLeaderId,
            Status = WorkflowStatus.Draft
        };

        foreach (var userId in request.MemberUserIds.Distinct())
        {
            project.Members.Add(new ProjectMember
            {
                ProjectId = project.Id,
                UserId = userId
            });
        }

        _db.Projects.Add(project);
        await _db.SaveChangesAsync(ct);

        // Load details for returning
        var loaded = await _db.Projects
            .Include(x => x.Leader)
            .Include(x => x.SubLeader)
            .Include(x => x.Members)
                .ThenInclude(m => m.User)
                    .ThenInclude(u => u!.Department)
            .FirstAsync(x => x.Id == project.Id, ct);

        var memberDtos = loaded.Members.Select(m => new ProjectMemberDto(
            m.UserId,
            m.User?.FullName ?? "Thành viên",
            m.User?.Department?.Name ?? "Phòng ban"
        )).ToList();

        return new ProjectDetailDto(
            loaded.Id,
            loaded.Name,
            loaded.Description,
            loaded.LeaderId,
            loaded.Leader?.FullName ?? "Tổ trưởng",
            loaded.SubLeaderId,
            loaded.SubLeader?.FullName,
            loaded.Status.ToString().ToLowerInvariant(),
            loaded.SubmittedAt,
            loaded.ApprovedAt,
            loaded.Members.Select(m => m.UserId).ToList(),
            memberDtos,
            loaded.CreatedAt,
            loaded.UpdatedAt);
    }
}
