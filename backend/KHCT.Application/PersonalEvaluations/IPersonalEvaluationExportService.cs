namespace KHCT.Application.PersonalEvaluations;

public record PersonalEvaluationExportResult(string FileName, string ContentType, byte[] Content);

public interface IPersonalEvaluationExportService
{
    System.Threading.Tasks.Task<PersonalEvaluationExportResult> ExportPhuLuc01Async(Guid periodId, CancellationToken ct);
    System.Threading.Tasks.Task<PersonalEvaluationExportResult> ExportPhuLuc01AAsync(Guid periodId, CancellationToken ct);
}
