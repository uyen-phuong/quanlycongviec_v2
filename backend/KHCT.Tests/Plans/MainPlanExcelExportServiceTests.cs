using System.IO.Compression;
using System.Text;
using FluentAssertions;
using KHCT.Domain.Entities;
using KHCT.Domain.Enums;
using KHCT.Infrastructure.Excel;
using TaskEntity = KHCT.Domain.Entities.Task;

namespace KHCT.Tests.Plans;

public class MainPlanExcelExportServiceTests
{
    [Fact]
    public async System.Threading.Tasks.Task ExportAsync_ShouldCreateWorkbook_WithSummaryAndDetailSheets()
    {
        var service = new MainPlanExcelExportService();
        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Scope = PlanScope.Main,
            Year = 2026,
            Month = 5
        };
        var tasks = new List<TaskEntity>
        {
            new()
            {
                Id = Guid.NewGuid(),
                PlanId = plan.Id,
                OutlineIndex = "1",
                Title = "Cong viec 1",
                BksMemberText = "BKS 1",
                KtnbLeaderText = "Lanh dao 1",
                WorkType = WorkType.General,
                WorkStatus = WorkStatus.InProgress,
                Deadline = new DateTime(2026, 5, 20),
                ProgressText = "Dang lam",
                ReasonNotCompleted = "Cho du lieu",
                NoteText = "Ghi chu 1",
                OwnerDepartment = new Department { Code = "KTNB1", Name = "KTNB1" }
            }
        };

        var result = await service.ExportAsync(plan, tasks, CancellationToken.None);

        result.FileName.Should().Be("khct-main-2026-05.xlsx");
        using var archive = new ZipArchive(new MemoryStream(result.Content), ZipArchiveMode.Read);
        var workbookXml = ReadEntry(archive, "xl/workbook.xml");
        var sharedStringsXml = ReadEntry(archive, "xl/sharedStrings.xml");

        workbookXml.Should().Contain("Bao cao");
        workbookXml.Should().Contain("Chi tiet");
        sharedStringsXml.Should().Contain("Han hoan thanh");
        sharedStringsXml.Should().Contain("Lanh dao KTNB chi dao");
        sharedStringsXml.Should().Contain("BKS 1");
        sharedStringsXml.Should().Contain("Lanh dao 1");
        sharedStringsXml.Should().Contain("Dang lam");
        sharedStringsXml.Should().Contain("Cho du lieu");
        sharedStringsXml.Should().Contain("Ghi chu 1");
        sharedStringsXml.Should().Contain("KtnbLeaderText");
        sharedStringsXml.Should().Contain("NoteText");
    }

    private static string ReadEntry(ZipArchive archive, string path)
    {
        using var stream = archive.GetEntry(path)!.Open();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}
