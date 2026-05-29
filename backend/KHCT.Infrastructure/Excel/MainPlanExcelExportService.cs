using ClosedXML.Excel;
using KHCT.Application.Common.Interfaces;
using KHCT.Application.Plans.Export;
using KHCT.Domain.Entities;
using TaskEntity = KHCT.Domain.Entities.Task;

namespace KHCT.Infrastructure.Excel;

public sealed class MainPlanExcelExportService : IMainPlanExcelExportService
{
    public System.Threading.Tasks.Task<MainPlanExcelExportResult> ExportAsync(Plan plan, IReadOnlyList<TaskEntity> tasks, CancellationToken ct)
    {
        using var workbook = new XLWorkbook();
        BuildSummarySheet(workbook, plan, tasks);
        BuildDetailSheet(workbook, plan, tasks);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var fileName = $"khct-main-{plan.Year:D4}-{plan.Month:D2}.xlsx";
        return System.Threading.Tasks.Task.FromResult(new MainPlanExcelExportResult(
            fileName,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            stream.ToArray()));
    }

    private static void BuildSummarySheet(XLWorkbook workbook, Plan plan, IReadOnlyList<TaskEntity> tasks)
    {
        var ws = workbook.Worksheets.Add("Bao cao");
        ws.Style.Font.FontName = "Times New Roman";
        ws.Style.Font.FontSize = 12;
        ws.SheetView.FreezeRows(3);

        ws.Range("A1:I1").Merge().Value = $"THEO DOI TIEN DO THUC HIEN KE HOACH CONG TAC THANG {plan.Month:D2}/{plan.Year:D4}";
        ws.Cell("A2").Value = $"Xuat ngay: {DateTime.Now:dd/MM/yyyy HH:mm}";

        var headers = new[]
        {
            "TT",
            "Noi dung cong viec",
            "Thanh vien BKS chi dao",
            "Lanh dao KTNB chi dao",
            "Han hoan thanh",
            "Tien do thuc hien",
            "Nguyen nhan chua hoan thanh",
            "Ghi chu",
            "Phong dau moi"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            ws.Cell(3, i + 1).Value = headers[i];
        }

        StyleTitle(ws.Range("A1:I1"));
        StyleHeader(ws.Range("A3:I3"));

        var row = 4;
        foreach (var task in tasks)
        {
            ws.Cell(row, 1).Value = task.OutlineIndex;
            ws.Cell(row, 2).Value = task.Title;
            ws.Cell(row, 3).Value = task.BksMemberText;
            ws.Cell(row, 4).Value = task.KtnbLeaderText;
            ws.Cell(row, 5).Value = task.Deadline;
            ws.Cell(row, 6).Value = task.ProgressText;
            ws.Cell(row, 7).Value = task.ReasonNotCompleted;
            ws.Cell(row, 8).Value = task.NoteText;
            ws.Cell(row, 9).Value = task.OwnerDepartment?.Name;

            ws.Cell(row, 5).Style.DateFormat.Format = "dd/MM/yyyy";
            if (task.IsHeader)
            {
                ws.Range(row, 1, row, 9).Style.Font.Bold = true;
                ws.Range(row, 1, row, 9).Style.Fill.BackgroundColor = XLColor.FromHtml("#EAF2F8");
            }

            row++;
        }

        var dataRange = row > 4 ? ws.Range(3, 1, row - 1, 9) : ws.Range(3, 1, 3, 9);
        dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
        dataRange.Style.Alignment.WrapText = true;
        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        ws.Column(1).Width = 10;
        ws.Column(2).Width = 55;
        ws.Column(3).Width = 28;
        ws.Column(4).Width = 28;
        ws.Column(5).Width = 16;
        ws.Column(6).Width = 40;
        ws.Column(7).Width = 35;
        ws.Column(8).Width = 30;
        ws.Column(9).Width = 22;
    }

    private static void BuildDetailSheet(XLWorkbook workbook, Plan plan, IReadOnlyList<TaskEntity> tasks)
    {
        var ws = workbook.Worksheets.Add("Chi tiet");
        ws.Style.Font.FontName = "Times New Roman";
        ws.Style.Font.FontSize = 11;

        var headers = new[]
        {
            "TaskId",
            "ParentTaskId",
            "OutlineIndex",
            "Title",
            "IsHeader",
            "DisplayOrder",
            "WorkStatus",
            "Deadline",
            "KtnbLeaderText",
            "ProgressText",
            "ReasonNotCompleted",
            "NoteText",
            "OwnerDepartment",
            "SupportingDepartments"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
        }

        StyleHeader(ws.Range(1, 1, 1, headers.Length));

        var row = 2;
        foreach (var task in tasks)
        {
            ws.Cell(row, 1).Value = task.Id.ToString();
            ws.Cell(row, 2).Value = task.ParentTaskId?.ToString();
            ws.Cell(row, 3).Value = task.OutlineIndex;
            ws.Cell(row, 4).Value = task.Title;
            ws.Cell(row, 5).Value = task.IsHeader;
            ws.Cell(row, 6).Value = task.DisplayOrder;
            ws.Cell(row, 7).Value = task.WorkStatus.ToString();
            ws.Cell(row, 8).Value = task.Deadline;
            ws.Cell(row, 9).Value = task.KtnbLeaderText;
            ws.Cell(row, 10).Value = task.ProgressText;
            ws.Cell(row, 11).Value = task.ReasonNotCompleted;
            ws.Cell(row, 12).Value = task.NoteText;
            ws.Cell(row, 13).Value = task.OwnerDepartment?.Name;
            ws.Cell(row, 14).Value = string.Join(", ", task.SupportingDepts
                .Where(x => x.Department is not null)
                .Select(x => x.Department!.Code));
            ws.Cell(row, 8).Style.DateFormat.Format = "dd/MM/yyyy";
            row++;
        }

        var dataRange = row > 2 ? ws.Range(1, 1, row - 1, headers.Length) : ws.Range(1, 1, 1, headers.Length);
        dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
        dataRange.Style.Alignment.WrapText = true;
        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        ws.Column(1).Width = 38;
        ws.Column(2).Width = 38;
        ws.Column(3).Width = 14;
        ws.Column(4).Width = 50;
        ws.Column(5).Width = 12;
        ws.Column(6).Width = 14;
        ws.Column(7).Width = 18;
        ws.Column(8).Width = 16;
        ws.Column(9).Width = 28;
        ws.Column(10).Width = 36;
        ws.Column(11).Width = 32;
        ws.Column(12).Width = 28;
        ws.Column(13).Width = 20;
        ws.Column(14).Width = 24;
    }

    private static void StyleTitle(IXLRange range)
    {
        range.Style.Font.Bold = true;
        range.Style.Font.FontSize = 14;
        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        range.Style.Fill.BackgroundColor = XLColor.FromHtml("#D9EAF7");
        range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    }

    private static void StyleHeader(IXLRange range)
    {
        range.Style.Font.Bold = true;
        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        range.Style.Fill.BackgroundColor = XLColor.FromHtml("#F4F6F6");
        range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        range.Style.Alignment.WrapText = true;
    }
}
