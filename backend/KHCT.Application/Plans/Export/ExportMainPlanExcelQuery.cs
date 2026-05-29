using KHCT.Application.Common.Interfaces;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Plans.Export;

public record ExportMainPlanExcelQuery(Guid PlanId) : IRequest<MainPlanExcelExportResult>;

public class ExportMainPlanExcelHandler : IRequestHandler<ExportMainPlanExcelQuery, MainPlanExcelExportResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IMainPlanExcelExportService _exportService;

    public ExportMainPlanExcelHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser,
        IMainPlanExcelExportService exportService)
    {
        _db = db;
        _currentUser = currentUser;
        _exportService = exportService;
    }

    public async Task<MainPlanExcelExportResult> Handle(ExportMainPlanExcelQuery request, CancellationToken ct)
    {
        var plan = await _db.Plans
            .AsNoTracking()
            .Include(x => x.Department)
            .Include(x => x.CreatedBy)
            .FirstOrDefaultAsync(x => x.Id == request.PlanId && x.Scope == PlanScope.Main, ct)
            ?? throw new KeyNotFoundException("Plan not found.");

        var tasks = await _db.Tasks
            .AsNoTracking()
            .Include(x => x.AssigneeUser)
            .Include(x => x.OwnerDepartment)
            .Include(x => x.SupportingDepts)
                .ThenInclude(x => x.Department)
            .Where(x => x.PlanId == plan.Id && x.WorkType == WorkType.General)
            .OrderBy(x => x.ParentTaskId.HasValue ? 1 : 0)
            .ThenBy(x => x.ParentTaskId)
            .ThenBy(x => x.DisplayOrder)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(ct);

        return await _exportService.ExportAsync(plan, tasks, ct);
    }
}
