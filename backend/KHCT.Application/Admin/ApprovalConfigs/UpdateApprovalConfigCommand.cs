using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Application.Common.Support;
using KHCT.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Admin.ApprovalConfigs;

public record UpdateApprovalConfigCommand(
    Guid Id,
    Guid? DepartmentId,
    Guid RoleId,
    bool IsActive) : IRequest<ApprovalConfigDto>;

public class UpdateApprovalConfigCommandValidator : AbstractValidator<UpdateApprovalConfigCommand>
{
    public UpdateApprovalConfigCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.RoleId).NotEmpty();
    }
}

public class UpdateApprovalConfigHandler : IRequestHandler<UpdateApprovalConfigCommand, ApprovalConfigDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public UpdateApprovalConfigHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ApprovalConfigDto> Handle(UpdateApprovalConfigCommand request, CancellationToken ct)
    {
        var config = await _db.ApprovalConfigs
            .Include(x => x.Department)
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new KeyNotFoundException("Approval config not found.");

        var duplicateExists = await _db.ApprovalConfigs
            .AnyAsync(x => x.Id != config.Id && x.Scope == config.Scope && x.Level == config.Level, ct);
        if (duplicateExists)
        {
            throw new DomainException("approval_config_duplicate", "Approval config must be unique by scope and level.");
        }

        var role = await AdminSupport.RequireRoleAsync(_db, request.RoleId, ct);
        var department = await ApplicationSupport.RequireActiveDepartmentAsync(_db, request.DepartmentId, ct);

        config.RoleId = role.Id;
        config.Role = role;
        config.DepartmentId = department?.Id;
        config.Department = department;
        config.IsActive = request.IsActive;

        await _db.SaveChangesAsync(ct);
        return AdminSupport.ToDto(config);
    }
}
