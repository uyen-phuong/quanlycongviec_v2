using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Domain.Common;
using KHCT.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Admin.Departments;

public record CreateDepartmentCommand(string Code, string Name) : IRequest<DepartmentDto>;

public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
    }
}

public class CreateDepartmentHandler : IRequestHandler<CreateDepartmentCommand, DepartmentDto>
{
    private readonly IApplicationDbContext _db;

    public CreateDepartmentHandler(IApplicationDbContext db) => _db = db;

    public async Task<DepartmentDto> Handle(CreateDepartmentCommand request, CancellationToken ct)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        if (await _db.Departments.AnyAsync(x => x.Code == code, ct))
            throw new DomainException("dept_code_taken", "Department code already exists.");

        var dept = new Department
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = request.Name.Trim(),
            IsActive = true
        };

        _db.Departments.Add(dept);
        await _db.SaveChangesAsync(ct);
        return AdminSupport.ToDto(dept);
    }
}
