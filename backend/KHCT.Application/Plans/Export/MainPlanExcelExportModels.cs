namespace KHCT.Application.Plans.Export;

public record MainPlanExcelExportResult(
    string FileName,
    string ContentType,
    byte[] Content);
