namespace KHCT.Application.Plans.Import;

public record MainPlanExcelImportRow(
    int RowNumber,
    string OutlineIndex,
    int Level,
    string Title,
    bool IsHeader,
    string? BksMemberText,
    string? KtnbLeaderText,
    string? NoteText,
    string? ProgressText,
    string? ReasonNotCompleted);

public record MainPlanExcelImportData(
    string SheetName,
    int HeaderRowNumber,
    IReadOnlyList<MainPlanExcelImportRow> Rows);

public record ImportMainPlanExcelResultDto(
    bool Success,
    string FileName,
    string SheetName,
    int HeaderRowNumber,
    int TotalRows,
    int HeaderRows,
    int TaskRows,
    int ReplacedTasks);
