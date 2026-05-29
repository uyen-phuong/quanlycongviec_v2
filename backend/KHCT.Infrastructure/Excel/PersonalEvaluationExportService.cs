using ClosedXML.Excel;
using KHCT.Application.Common.Interfaces;
using KHCT.Application.PersonalEvaluations;
using KHCT.Domain.Common;
using KHCT.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Infrastructure.Excel;

public sealed class PersonalEvaluationExportService : IPersonalEvaluationExportService
{
    private static readonly string TemplatesDir = Path.Combine(AppContext.BaseDirectory, "Excel", "Templates");

    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public PersonalEvaluationExportService(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    private sealed record Context(
        PersonalEvaluationPeriod Period,
        List<PersonalEvaluationItem> Items,
        string UserRoleCode,
        string? TeamLeadName,   // TRUONG_NHOM cùng phòng
        string? ManagerName,    // TRUONG_PHONG cùng phòng
        string? DeputyName,     // PHO_TRUONG_KTNB
        string? HeadName);      // TRUONG_KTNB

    private async System.Threading.Tasks.Task<Context> LoadContextAsync(Guid periodId, CancellationToken ct)
    {
        var period = await _db.PersonalEvaluationPeriods
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Department)
            .FirstOrDefaultAsync(x => x.Id == periodId, ct)
            ?? throw new KeyNotFoundException("period not found");

        if (!PersonalEvaluationSupport.CanReadPeriodOf(_currentUser, period.User!))
            throw new ForbiddenException(PersonalEvaluationSupport.ForbiddenRole, "Không có quyền xuất phụ lục này.");

        var items = await _db.PersonalEvaluationItems
            .AsNoTracking()
            .Where(x => x.PeriodId == periodId)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync(ct);

        var userRoleCode = await (
            from ur in _db.UserRoles
            join r in _db.Roles on ur.RoleId equals r.Id
            where ur.UserId == period.UserId
            select r.Code
        ).FirstOrDefaultAsync(ct) ?? string.Empty;

        var deptId = period.DepartmentId;
        var leaderData = await (
            from ur in _db.UserRoles
            join u in _db.Users on ur.UserId equals u.Id
            join r in _db.Roles on ur.RoleId equals r.Id
            where u.IsActive &&
                  (r.Code == "TRUONG_NHOM" || r.Code == "TRUONG_PHONG" ||
                   r.Code == "PHO_TRUONG_KTNB" || r.Code == "TRUONG_KTNB")
            select new { u.FullName, u.DepartmentId, RoleCode = r.Code }
        ).ToListAsync(ct);

        return new Context(
            Period: period,
            Items: items,
            UserRoleCode: userRoleCode,
            TeamLeadName: leaderData.FirstOrDefault(x => x.RoleCode == "TRUONG_NHOM" && x.DepartmentId == deptId)?.FullName,
            ManagerName:  leaderData.FirstOrDefault(x => x.RoleCode == "TRUONG_PHONG" && x.DepartmentId == deptId)?.FullName,
            DeputyName:   leaderData.FirstOrDefault(x => x.RoleCode == "PHO_TRUONG_KTNB")?.FullName,
            HeadName:     leaderData.FirstOrDefault(x => x.RoleCode == "TRUONG_KTNB")?.FullName);
    }

    // ---------------------------------------------------------------------------
    // Phụ lục 01
    // ---------------------------------------------------------------------------

    public async System.Threading.Tasks.Task<PersonalEvaluationExportResult> ExportPhuLuc01Async(Guid periodId, CancellationToken ct)
    {
        var ctx = await LoadContextAsync(periodId, ct);
        var period = ctx.Period;
        var items  = ctx.Items;
        var deptName = period.Department?.Name ?? string.Empty;
        var userName = period.User?.FullName ?? string.Empty;

        var templatePath = Path.Combine(TemplatesDir, "PhuLuc01.xlsx");
        using var workbook = new XLWorkbook(templatePath);
        var ws = workbook.Worksheets.First();

        // --- Metadata ---
        ws.Cell("A5").Value = $"PHÒNG: {deptName.ToUpperInvariant()}";
        ws.Cell("A7").Value = $"(kỳ tạm ứng thù lao theo hiệu quả V2 tháng {period.ReportMonth:D2} năm {period.ReportYear})";
        ws.Cell("A9").Value = $"Họ và tên cán bộ/cán bộ trưng tập: {userName}";

        // Template layout constants
        const int firstDataRow   = 16;
        const int templateDataRows = 7;   // rows 16-22 pre-allocated
        const int templateAvgRow = 23;
        const int templateSigNamesRow = 31;

        int n = items.Count;

        // Clear sample data from template rows (preserve formatting)
        for (int r = firstDataRow; r < firstDataRow + templateDataRows; r++)
            for (int c = 1; c <= 20; c++)
                ws.Cell(r, c).Clear(XLClearOptions.Contents);
        // Clear avg formulas
        for (int c = 1; c <= 20; c++)
            ws.Cell(templateAvgRow, c).Clear(XLClearOptions.Contents);

        // Insert extra rows if more items than template allows
        int extraRows = Math.Max(0, n - templateDataRows);
        if (extraRows > 0)
            ws.Row(templateAvgRow).InsertRowsAbove(extraRows);

        // --- Write item rows ---
        for (int i = 0; i < n; i++)
        {
            int row = firstDataRow + i;
            var it = items[i];
            ws.Cell(row, 1).Value = i + 1;
            ws.Cell(row, 2).Value = it.AssignmentSource ?? string.Empty;
            ws.Cell(row, 3).Value = it.TaskName ?? string.Empty;
            ws.Cell(row, 4).Value = it.TaskDetail ?? string.Empty;
            if (it.Deadline.HasValue)    ws.Cell(row, 5).Value = it.Deadline.Value.Date;
            ws.Cell(row, 6).Value = it.ActualResult ?? string.Empty;
            if (it.CompletedAt.HasValue) ws.Cell(row, 7).Value = it.CompletedAt.Value.Date;
            SetDecimal(ws, row,  8, it.SelfProgressScore);
            SetDecimal(ws, row,  9, it.SelfQualityScore);
            SetDecimal(ws, row, 10, it.TeamLeadProgressScore);
            SetDecimal(ws, row, 11, it.TeamLeadQualityScore);
            SetDecimal(ws, row, 12, it.ManagerProgressScore);
            SetDecimal(ws, row, 13, it.ManagerQualityScore);
            SetDecimal(ws, row, 14, it.DeputyProgressScore);
            SetDecimal(ws, row, 15, it.DeputyQualityScore);
            SetDecimal(ws, row, 16, it.HeadProgressScore);
            SetDecimal(ws, row, 17, it.HeadQualityScore);
            // Cols 18-19: per-row averages across scorers
            SetDecimal(ws, row, 18, AvgOf(it.SelfProgressScore, it.TeamLeadProgressScore, it.ManagerProgressScore, it.DeputyProgressScore, it.HeadProgressScore));
            SetDecimal(ws, row, 19, AvgOf(it.SelfQualityScore,  it.TeamLeadQualityScore,  it.ManagerQualityScore,  it.DeputyQualityScore,  it.HeadQualityScore));
            ws.Cell(row, 20).Value = it.Note ?? string.Empty;
        }

        // --- Avg row (column averages across all items) ---
        int avgRow = firstDataRow + Math.Max(n, templateDataRows);
        if (n > 0)
        {
            SetDecimal(ws, avgRow,  8, AvgSeq(items, x => x.SelfProgressScore));
            SetDecimal(ws, avgRow,  9, AvgSeq(items, x => x.SelfQualityScore));
            SetDecimal(ws, avgRow, 10, AvgSeq(items, x => x.TeamLeadProgressScore));
            SetDecimal(ws, avgRow, 11, AvgSeq(items, x => x.TeamLeadQualityScore));
            SetDecimal(ws, avgRow, 12, AvgSeq(items, x => x.ManagerProgressScore));
            SetDecimal(ws, avgRow, 13, AvgSeq(items, x => x.ManagerQualityScore));
            SetDecimal(ws, avgRow, 14, AvgSeq(items, x => x.DeputyProgressScore));
            SetDecimal(ws, avgRow, 15, AvgSeq(items, x => x.DeputyQualityScore));
            SetDecimal(ws, avgRow, 16, AvgSeq(items, x => x.HeadProgressScore));
            SetDecimal(ws, avgRow, 17, AvgSeq(items, x => x.HeadQualityScore));
            SetDecimal(ws, avgRow, 18, AvgSeq(items, x => AvgOf(x.SelfProgressScore, x.TeamLeadProgressScore, x.ManagerProgressScore, x.DeputyProgressScore, x.HeadProgressScore)));
            SetDecimal(ws, avgRow, 19, AvgSeq(items, x => AvgOf(x.SelfQualityScore,  x.TeamLeadQualityScore,  x.ManagerQualityScore,  x.DeputyQualityScore,  x.HeadQualityScore)));
        }

        // --- Signature names ---
        // Template row 31 has names; shifts by extraRows after insertion
        int sigNamesRow = templateSigNamesRow + extraRows;
        ws.Cell(sigNamesRow, 2).Value  = userName;                                    // B = Người lao động
        WriteIfNotEmpty(ws, sigNamesRow,  7, ctx.ManagerName);                        // G = Lãnh đạo phòng
        WriteIfNotEmpty(ws, sigNamesRow, 13, ctx.DeputyName);                         // M = Phó Trưởng KTNB
        WriteIfNotEmpty(ws, sigNamesRow, 17, ctx.HeadName);                           // Q = Trưởng KTNB

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var fileName = $"PhuLuc01-{Sanitize(userName)}-{period.ReportYear:D4}-{period.ReportMonth:D2}.xlsx";
        return new PersonalEvaluationExportResult(fileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", stream.ToArray());
    }

    // ---------------------------------------------------------------------------
    // Phụ lục 01A
    // ---------------------------------------------------------------------------

    public async System.Threading.Tasks.Task<PersonalEvaluationExportResult> ExportPhuLuc01AAsync(Guid periodId, CancellationToken ct)
    {
        var ctx = await LoadContextAsync(periodId, ct);
        var period   = ctx.Period;
        var items    = ctx.Items;
        var deptName = period.Department?.Name ?? string.Empty;
        var userName = period.User?.FullName    ?? string.Empty;
        var position = RoleToPosition(ctx.UserRoleCode);
        var now      = DateTime.Now;

        var templatePath = Path.Combine(TemplatesDir, "PhuLuc01A.xlsx");
        using var workbook = new XLWorkbook(templatePath);
        var ws = workbook.Worksheets.First();

        // --- Metadata ---
        ws.Cell("F4").Value  = $"Hà Nội, ngày {now.Day} tháng {now.Month} năm {now.Year}";
        ws.Cell("A6").Value  = $"PHÒNG: {deptName.ToUpperInvariant()}";
        ws.Cell("A8").Value  = $"Kỳ tạm ứng thù lao theo hiệu quả V2 tháng {period.ReportMonth:D2} Năm {period.ReportYear}";
        ws.Cell("A10").Value = $"Họ và tên: {userName}";
        ws.Cell("A11").Value = $"Chức vụ: {position}";
        ws.Cell("A12").Value = $"Phòng: {deptName}";

        // Averages from items (per scorer column)
        var selfProg  = AvgSeq(items, x => x.SelfProgressScore);
        var teamProg  = AvgSeq(items, x => x.TeamLeadProgressScore);
        var mgrProg   = AvgSeq(items, x => x.ManagerProgressScore);
        var depProg   = AvgSeq(items, x => x.DeputyProgressScore);
        var headProg  = AvgSeq(items, x => x.HeadProgressScore);

        var selfQual  = AvgSeq(items, x => x.SelfQualityScore);
        var teamQual  = AvgSeq(items, x => x.TeamLeadQualityScore);
        var mgrQual   = AvgSeq(items, x => x.ManagerQualityScore);
        var depQual   = AvgSeq(items, x => x.DeputyQualityScore);
        var headQual  = AvgSeq(items, x => x.HeadQualityScore);

        // --- Score rows (cols C=3..G=7: self, teamLead, manager, deputy, head) ---
        // Row 18: Khối lượng công việc — avg progress per scorer
        WriteScoreRow(ws, 18, selfProg, teamProg, mgrProg, depProg, headProg);
        // Row 19: Chất lượng công việc — avg quality per scorer
        WriteScoreRow(ws, 19, selfQual, teamQual, mgrQual, depQual, headQual);
        // Row 20: Tiến độ thực hiện — avg progress per scorer
        WriteScoreRow(ws, 20, selfProg, teamProg, mgrProg, depProg, headProg);
        // Row 21: Năng lực, thái độ — period-level per scorer
        WriteScoreRow(ws, 21, period.CapacityAttitudeSelfScore, period.CapacityAttitudeTeamLeadScore, period.CapacityAttitudeManagerScore, period.CapacityAttitudeDeputyScore, period.CapacityAttitudeHeadScore);
        // Row 22: Kỷ luật nội quy — period-level
        WriteScoreRow(ws, 22, period.DisciplineSelfScore, period.DisciplineTeamLeadScore, period.DisciplineManagerScore, period.DisciplineDeputyScore, period.DisciplineHeadScore);
        // Row 23: Kiểm tra nghiệp vụ — period-level
        WriteScoreRow(ws, 23, period.InspectionSelfScore, period.InspectionTeamLeadScore, period.InspectionManagerScore, period.InspectionDeputyScore, period.InspectionHeadScore);

        // Row 17: Section I total = sum of rows 18-21
        for (int col = 3; col <= 7; col++)
            SetDecimal(ws, 17, col, SumRows(ws, [18, 19, 20, 21], col));

        // Row 24: Grand total = rows 17 + 22 + 23
        for (int col = 3; col <= 7; col++)
            SetDecimal(ws, 24, col, SumRows(ws, [17, 22, 23], col));

        // Col H (8): average of 5 scorer columns for rows 17-24
        for (int r = 17; r <= 24; r++)
        {
            var vals = Enumerable.Range(3, 5)
                .Select(col => TryGetDecimal(ws, r, col, out var v) ? (decimal?)v : null)
                .Where(x => x.HasValue).Select(x => x!.Value).ToList();
            SetDecimal(ws, r, 8, vals.Count > 0 ? Math.Round(vals.Average(), 1) : null);
        }

        // --- Signature names (row 33) ---
        ws.Cell("A33").Value = userName;
        WriteIfNotEmpty(ws, 33, 3, ctx.ManagerName);   // C = Lãnh đạo phòng (TRUONG_PHONG)
        WriteIfNotEmpty(ws, 33, 6, ctx.DeputyName);    // F = P. Trưởng KTNB phụ trách
        WriteIfNotEmpty(ws, 33, 8, ctx.HeadName);      // H = Trưởng KTNB

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var fileName = $"PhuLuc01A-{Sanitize(userName)}-{period.ReportYear:D4}-{period.ReportMonth:D2}.xlsx";
        return new PersonalEvaluationExportResult(fileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", stream.ToArray());
    }

    // ---------------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------------

    private static void WriteScoreRow(IXLWorksheet ws, int row,
        decimal? self, decimal? team, decimal? manager, decimal? deputy, decimal? head)
    {
        SetDecimal(ws, row, 3, self);
        SetDecimal(ws, row, 4, team);
        SetDecimal(ws, row, 5, manager);
        SetDecimal(ws, row, 6, deputy);
        SetDecimal(ws, row, 7, head);
    }

    private static decimal? SumRows(IXLWorksheet ws, int[] rows, int col)
    {
        decimal sum = 0; bool any = false;
        foreach (var r in rows)
            if (TryGetDecimal(ws, r, col, out var v)) { sum += v; any = true; }
        return any ? sum : null;
    }

    private static void SetDecimal(IXLWorksheet ws, int row, int col, decimal? value)
    {
        if (value.HasValue) ws.Cell(row, col).Value = (double)value.Value;
        else ws.Cell(row, col).Clear(XLClearOptions.Contents);
    }

    private static bool TryGetDecimal(IXLWorksheet ws, int row, int col, out decimal value)
    {
        value = 0;
        var cell = ws.Cell(row, col);
        if (cell.IsEmpty()) return false;
        if (cell.TryGetValue<double>(out var d)) { value = (decimal)d; return true; }
        return false;
    }

    private static void WriteIfNotEmpty(IXLWorksheet ws, int row, int col, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value)) ws.Cell(row, col).Value = value;
    }

    private static decimal? AvgOf(params decimal?[] values)
    {
        var vals = values.Where(x => x.HasValue).Select(x => x!.Value).ToList();
        return vals.Count == 0 ? null : Math.Round(vals.Average(), 1);
    }

    private static decimal? AvgSeq(IEnumerable<PersonalEvaluationItem> items, Func<PersonalEvaluationItem, decimal?> selector)
    {
        var vals = items.Select(selector).Where(x => x.HasValue).Select(x => x!.Value).ToList();
        return vals.Count == 0 ? null : Math.Round(vals.Average(), 1);
    }

    private static string RoleToPosition(string roleCode) => roleCode switch
    {
        "TRUONG_NHOM"      => "Trưởng nhóm",
        "TRUONG_PHONG"     => "Trưởng phòng",
        "PHO_TRUONG_KTNB"  => "Phó Trưởng KTNB",
        "TRUONG_KTNB"      => "Trưởng KTNB",
        "VAN_THU"          => "Văn thư",
        _                  => "Nhân viên"
    };

    private static string Sanitize(string s) =>
        new string(s.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_').ToArray());
}
