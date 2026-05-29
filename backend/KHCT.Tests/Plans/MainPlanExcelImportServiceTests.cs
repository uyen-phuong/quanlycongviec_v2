using ClosedXML.Excel;
using FluentAssertions;
using KHCT.Infrastructure.Excel;

namespace KHCT.Tests.Plans;

public class MainPlanExcelImportServiceTests
{
    [Fact]
    public async System.Threading.Tasks.Task ParseAsync_ShouldUseMergedTitleAndSkipFooterRows()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.AddWorksheet("PL3. TH KHCT (2)");

        ws.Cell(3, 1).Value = "TT";
        ws.Cell(3, 2).Value = "Nội dung văn bản chỉ đạo";

        ws.Cell(57, 1).Value = "4";
        ws.Cell(57, 2).Value = "Cong viec merge title";
        ws.Cell(57, 3).Value = "BKS A";
        ws.Cell(58, 6).Value = "Tien do dong duoi";
        ws.Range("A57:A58").Merge();
        ws.Range("B57:B58").Merge();

        ws.Cell(201, 1).Value = "Thêm mục";
        ws.Cell(203, 6).Value = "PHÓ PHÒNG PHỤ TRÁCH";

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        var service = new MainPlanExcelImportService();
        var result = await service.ParseAsync("sample.xlsx", stream.ToArray(), CancellationToken.None);

        result.Rows.Should().ContainSingle();
        result.Rows[0].RowNumber.Should().Be(57);
        result.Rows[0].Title.Should().Be("Cong viec merge title");
        result.Rows[0].ProgressText.Should().Be("Tien do dong duoi");
    }
}
