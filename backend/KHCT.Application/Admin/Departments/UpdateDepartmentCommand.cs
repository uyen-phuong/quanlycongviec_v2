using FluentValidation;
using KHCT.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Admin.Departments;

public record UpdateDepartmentCommand(Guid Id, string Name, bool IsActive) : IRequest<DepartmentDto>;

public class UpdateDepartmentCommandValidator : AbstractValidator<UpdateDepartmentCommand>
{
    public UpdateDepartmentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
    }
}

public class UpdateDepartmentHandler : IRequestHandler<UpdateDepartmentCommand, DepartmentDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public UpdateDepartmentHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<DepartmentDto> Handle(UpdateDepartmentCommand request, CancellationToken ct)
    {
        var department = await _db.Departments
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new KeyNotFoundException("Department not found.");

        department.Name = request.Name.Trim();
        department.IsActive = request.IsActive;

        await _db.SaveChangesAsync(ct);
        return AdminSupport.ToDto(department);
    }
}
