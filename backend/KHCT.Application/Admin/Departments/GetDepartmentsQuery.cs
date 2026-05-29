using KHCT.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Admin.Departments;

public record GetDepartmentsQuery() : IRequest<IReadOnlyList<DepartmentDto>>;

public class GetDepartmentsHandler : IRequestHandler<GetDepartmentsQuery, IReadOnlyList<DepartmentDto>>
{
    private readonly IApplicationDbContext _db;

    public GetDepartmentsHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<DepartmentDto>> Handle(GetDepartmentsQuery request, CancellationToken ct)
    {
        var items = await _db.Departments
            .AsNoTracking()
            .OrderBy(x => x.Code)
            .ToListAsync(ct);

        return items.Select(AdminSupport.ToDto).ToList();
    }
}
