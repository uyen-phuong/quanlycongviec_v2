using KHCT.Domain.Entities;
using KHCT.Application.Plans.Export;

namespace KHCT.Application.Common.Interfaces;

public interface IMainPlanExcelExportService
{
    System.Threading.Tasks.Task<MainPlanExcelExportResult> ExportAsync(Plan plan, IReadOnlyList<KHCT.Domain.Entities.Task> tasks, CancellationToken ct);
}
