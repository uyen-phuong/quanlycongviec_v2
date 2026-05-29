using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using FluentValidation;
using FluentValidation.Results;
using KHCT.Application.Common.Interfaces;
using KHCT.Application.Plans.Import;

namespace KHCT.Infrastructure.Excel;

public sealed class MainPlanExcelImportService : IMainPlanExcelImportService
{
    private static readonly Regex LeadingLetterRegex = new(@"^\s*([a-zA-Z]\))\s*(.+)$", RegexOptions.Compiled);
    private static readonly Regex LeadingBulletRegex = new(@"^\s*([-•])\s*(.+)$", RegexOptions.Compiled);
    private static readonly HashSet<string> FooterMarkers =
    [
        "themmuc",
        "lapbieu",
        "phongthukytonghop",
        "phophongphutrach",
        "truongphong",
        "giamdoc",
        "kiemsoat",
        "pheduyet"
    ];

    public Task<MainPlanExcelImportData> ParseAsync(string fileName, byte[] content, CancellationToken ct)
    {
        if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            throw BuildValidation("file", "Invalid file format. Only .xlsx is supported.");
        }

        using var stream = new MemoryStream(content);
        using var workbook = new XLWorkbook(stream);
        if (workbook.Worksheets.Count == 0)
        {
            throw BuildValidation("file", "Invalid file. Workbook is empty.");
        }

        var worksheet = workbook.Worksheet(1);
        var headerRow = FindHeaderRow(worksheet);
        if (headerRow is null)
        {
            throw BuildValidation("file", "Invalid file. Header row was not found.");
        }

        var rows = ParseRows(worksheet, headerRow.Value + 1);
        if (rows.Count == 0)
        {
            throw BuildValidation("file", "Invalid file. No importable rows were found.");
        }

        return Task.FromResult(new MainPlanExcelImportData(worksheet.Name, headerRow.Value, rows));
    }

    private static int? FindHeaderRow(IXLWorksheet worksheet)
    {
        var maxRow = Math.Min(worksheet.LastRowUsed()?.RowNumber() ?? 0, 10);
        for (var row = 1; row <= maxRow; row++)
        {
            var colA = NormalizeHeader(worksheet.Cell(row, 1).GetString());
            var colB = NormalizeHeader(worksheet.Cell(row, 2).GetString());
            if (colA == "tt" && colB.Contains("noidungvanbanchidao", StringComparison.Ordinal))
            {
                return row;
            }
        }

        return null;
    }

    private static List<MainPlanExcelImportRow> ParseRows(IXLWorksheet worksheet, int dataStartRow)
    {
        var rows = new List<MainPlanExcelImportRow>();
        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? dataStartRow - 1;
        var previousLevel = 0;

        for (var rowNumber = dataStartRow; rowNumber <= lastRow; rowNumber++)
        {
            if (IsEmptyRow(worksheet, rowNumber))
            {
                continue;
            }

            var outlineCell = ReadMergedCellText(worksheet, rowNumber, 1);
            var titleCell = ReadMergedCellText(worksheet, rowNumber, 2) ?? string.Empty;
            if (ShouldSkipFooterRow(worksheet, rowNumber, outlineCell, titleCell))
            {
                continue;
            }

            var outline = outlineCell;
            var title = titleCell;
            var level = 0;

            if (!string.IsNullOrWhiteSpace(outlineCell))
            {
                outline = NormalizeOutline(outlineCell);
                level = InferLevelFromOutline(outline);
            }
            else
            {
                (outline, title, level) = ParseOutlineFromTitle(titleCell, previousLevel);
            }

            previousLevel = level;
            var bksMemberText = ReadMergedCellText(worksheet, rowNumber, 3);
            var ktnbLeaderText = ReadMergedCellText(worksheet, rowNumber, 4);
            var noteText = ReadMergedCellText(worksheet, rowNumber, 5);
            var progressText = ReadMergedCellText(worksheet, rowNumber, 6);
            var reasonText = ReadMergedCellText(worksheet, rowNumber, 11);

            var isHeader = string.IsNullOrWhiteSpace(bksMemberText) &&
                           string.IsNullOrWhiteSpace(ktnbLeaderText) &&
                           string.IsNullOrWhiteSpace(noteText) &&
                           string.IsNullOrWhiteSpace(progressText) &&
                           string.IsNullOrWhiteSpace(reasonText);

            rows.Add(new MainPlanExcelImportRow(
                rowNumber,
                outline,
                level,
                title ?? string.Empty,
                isHeader,
                bksMemberText,
                ktnbLeaderText,
                noteText,
                progressText,
                reasonText));
        }

        return rows;
    }

    private static (string Outline, string Title, int Level) ParseOutlineFromTitle(string title, int previousLevel)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return (string.Empty, string.Empty, Math.Max(previousLevel, 1));
        }

        var letterMatch = LeadingLetterRegex.Match(title);
        if (letterMatch.Success)
        {
            return (NormalizeOutline(letterMatch.Groups[1].Value), NormalizeCell(letterMatch.Groups[2].Value) ?? string.Empty, previousLevel + 1);
        }

        var bulletMatch = LeadingBulletRegex.Match(title);
        if (bulletMatch.Success)
        {
            return (bulletMatch.Groups[1].Value, NormalizeCell(bulletMatch.Groups[2].Value) ?? string.Empty, previousLevel + 1);
        }

        return (string.Empty, title, Math.Max(previousLevel, 1));
    }

    private static int InferLevelFromOutline(string outline)
    {
        var normalized = outline.Trim();
        if (Regex.IsMatch(normalized, @"^[A-Z]\.?$"))
        {
            return 1;
        }

        if (Regex.IsMatch(normalized, @"^(?i)(I|II|III|IV|V|VI|VII|VIII|IX|X)$"))
        {
            return 2;
        }

        var numeric = normalized.TrimEnd('.', ' ');
        if (Regex.IsMatch(numeric, @"^\d+(\.\d+)*$"))
        {
            return 2 + numeric.Split('.').Length;
        }

        if (Regex.IsMatch(normalized, @"^[a-zA-Z]\)$"))
        {
            return 5;
        }

        if (normalized is "-" or "•")
        {
            return 6;
        }

        return 3;
    }

    private static bool IsEmptyRow(IXLWorksheet worksheet, int rowNumber)
    {
        for (var col = 1; col <= 20; col++)
        {
            if (!string.IsNullOrWhiteSpace(ReadMergedCellText(worksheet, rowNumber, col)))
            {
                return false;
            }
        }

        return true;
    }

    private static string? ReadMergedCellText(IXLWorksheet worksheet, int rowNumber, int columnNumber)
    {
        var cell = worksheet.Cell(rowNumber, columnNumber);
        var direct = NormalizeCell(cell.GetString());
        if (!string.IsNullOrWhiteSpace(direct))
        {
            return direct;
        }

        var mergedRange = worksheet.MergedRanges.FirstOrDefault(range => range.Contains(cell));
        if (mergedRange is null)
        {
            return null;
        }

        return NormalizeCell(mergedRange.FirstCell().GetString());
    }

    private static bool ShouldSkipFooterRow(
        IXLWorksheet worksheet,
        int rowNumber,
        string? outlineCell,
        string titleCell)
    {
        var normalizedOutline = NormalizeHeader(outlineCell);
        var normalizedTitle = NormalizeHeader(titleCell);
        if (FooterMarkers.Contains(normalizedOutline) || FooterMarkers.Contains(normalizedTitle))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(titleCell))
        {
            return false;
        }

        for (var col = 3; col <= 20; col++)
        {
            var marker = NormalizeHeader(ReadMergedCellText(worksheet, rowNumber, col));
            if (FooterMarkers.Contains(marker))
            {
                return true;
            }
        }

        return false;
    }

    private static string NormalizeHeader(string? value) =>
        RemoveDiacritics((value ?? string.Empty).Trim().ToLowerInvariant())
            .Replace(" ", string.Empty)
            .Replace("\n", string.Empty);

    private static string NormalizeOutline(string value) =>
        Regex.Replace(value.Trim(), @"\s+", string.Empty);

    private static string? NormalizeCell(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Replace("\r\n", "\n").Replace('\r', '\n').Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static ValidationException BuildValidation(string field, string message) =>
        new([new ValidationFailure(field, message)]);

    private static string RemoveDiacritics(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(ch);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC)
            .Replace('đ', 'd')
            .Replace('Đ', 'D');
    }
}
