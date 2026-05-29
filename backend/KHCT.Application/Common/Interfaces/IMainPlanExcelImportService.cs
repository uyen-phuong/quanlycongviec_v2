using KHCT.Application.Plans.Import;

namespace KHCT.Application.Common.Interfaces;

public interface IMainPlanExcelImportService
{
    System.Threading.Tasks.Task<MainPlanExcelImportData> ParseAsync(string fileName, byte[] content, CancellationToken ct);
}
