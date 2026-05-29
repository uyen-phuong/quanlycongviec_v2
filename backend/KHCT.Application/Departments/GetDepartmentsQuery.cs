using KHCT.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Departments;

public record DepartmentLookupDto(Guid Id, string Code, string Name);

public record GetDepartmentsQuery() : IRequest<IReadOnlyList<DepartmentLookupDto>>;

public sealed class GetDepartmentsHandler : IRequestHandler<GetDepartmentsQuery, IReadOnlyList<DepartmentLookupDto>>
{
    private readonly IApplicationDbContext _db;

    public GetDepartmentsHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<DepartmentLookupDto>> Handle(GetDepartmentsQuery request, CancellationToken ct)
    {
        return await _db.Departments
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => new DepartmentLookupDto(x.Id, x.Code, x.Name))
            .ToListAsync(ct);
    }
}
