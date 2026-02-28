using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Diagnostics;
using Wolf.Desktop.Services;
using Wolf.Dtos;

namespace Wolf.Desktop.Inqueries;

public class ObligationsInquiry
{
    private readonly List<RequestDto> _requests;

    public ObligationsInquiry(List<RequestDto> requests)
    {
        _requests = requests;
    }

    public void ExportToExcel(string outputPath)
    {
        var cache = ServiceLocator.Cache;

        using (var document = SpreadsheetDocument.Create(outputPath, SpreadsheetDocumentType.Workbook))
        {
            var workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            var sheets = document.WorkbookPart!.Workbook.AppendChild(new Sheets());
            sheets.Append(new Sheet
            {
                Id = document.WorkbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "TasksInquiry"
            });

            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>()!;
            AddHeaderRow(sheetData);
            SetColumnWidths(worksheetPart);

            var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
            stylesPart.Stylesheet = CreateStylesheet();
            stylesPart.Stylesheet.Save();

            uint wrapStyle = 1;
            uint rowIndex = 2;

            foreach (var request in _requests)
            {
                var activities = cache.GetActivitiesForRequest(request.Requestid);
                foreach (var activity in activities)
                {
                    var tasks = cache.GetTasksForActivity(activity.Activityid);
                    foreach (var task in tasks)
                    {
                        if (!IsValidComment(task.Comments)) continue;

                        var row = new Row { RowIndex = rowIndex };
                        sheetData.Append(row);

                        AddTextCell(row, 1, task.Comments!, wrapStyle);

                        // Client names
                        var clients = cache.GetClientsForRequest(request.Requestid);
                        var clientNames = clients.Select(c =>
                            string.Join(" ", new[] { c.Firstname, c.Middlename, c.Lastname }
                                .Where(s => !string.IsNullOrEmpty(s))));
                        AddTextCell(row, 2, string.Join(", ", clientNames), wrapStyle);

                        // Plot numbers and owner names
                        var plots = cache.GetPlotsForActivity(activity.Activityid);
                        var plotNumbers = plots.Select(p => p.Plotnumber).ToList();
                        var ownerGroups = new List<string>();

                        foreach (var plot in plots)
                        {
                            var docs = cache.GetDocumentsForPlot(plot.Plotid);
                            var owners = docs.SelectMany(d => cache.GetOwnersForDocument(d.Documentid))
                                .Select(o => o.Fullname)
                                .Distinct()
                                .ToList();
                            ownerGroups.Add(owners.Count > 0 ? string.Join(", ", owners) : "Няма собственост");
                        }

                        AddTextCell(row, 3, string.Join(", ", plotNumbers), wrapStyle);
                        AddTextCell(row, 4, string.Join(" | ", ownerGroups), wrapStyle);
                        AddNumberCell(row, 5, activity.Employeepayment.ToString(), wrapStyle);

                        rowIndex++;
                    }
                }
            }

            worksheetPart.Worksheet.Save();
        }

        Process.Start(new ProcessStartInfo(outputPath) { UseShellExecute = true });
    }

    private static bool IsValidComment(string? comment)
    {
        if (string.IsNullOrEmpty(comment)) return false;
        var parts = comment.Split('-');
        return parts.Length == 3 && parts[0].Length == 2 && DateTime.TryParse(parts[2], out _);
    }

    private void AddHeaderRow(SheetData sheetData)
    {
        var row = new Row { RowIndex = 1 };
        sheetData.Append(row);
        AddTextCell(row, 1, "Номер и дата");
        AddTextCell(row, 2, "Възложители");
        AddTextCell(row, 3, "Имоти");
        AddTextCell(row, 4, "Собственици");
        AddTextCell(row, 5, "Сума на услугата");
    }

    private static void AddTextCell(Row row, int col, string value, uint styleIndex = 0)
    {
        uint ri = row.RowIndex?.Value ?? 0;
        row.Append(new Cell
        {
            CellReference = GetCellRef(col, (int)ri),
            DataType = CellValues.String,
            CellValue = new CellValue(value),
            StyleIndex = styleIndex
        });
    }

    private static void AddNumberCell(Row row, int col, string value, uint styleIndex = 0)
    {
        uint ri = row.RowIndex?.Value ?? 0;
        row.Append(new Cell
        {
            CellReference = GetCellRef(col, (int)ri),
            DataType = CellValues.Number,
            CellValue = new CellValue(value),
            StyleIndex = styleIndex
        });
    }

    private static string GetCellRef(int col, int rowIdx)
    {
        string colName = "";
        while (col > 0) { colName = (char)((col - 1) % 26 + 'A') + colName; col = (col - 1) / 26; }
        return $"{colName}{rowIdx}";
    }

    private static void SetColumnWidths(WorksheetPart wsp)
    {
        var cols = new Columns();
        cols.Append(new Column { Min = 1, Max = 1, Width = 25, CustomWidth = true });
        cols.Append(new Column { Min = 2, Max = 2, Width = 30, CustomWidth = true });
        cols.Append(new Column { Min = 3, Max = 3, Width = 25, CustomWidth = true });
        cols.Append(new Column { Min = 4, Max = 4, Width = 35, CustomWidth = true });
        cols.Append(new Column { Min = 5, Max = 5, Width = 20, CustomWidth = true });
        wsp.Worksheet.InsertAt(cols, 0);
    }

    private static Stylesheet CreateStylesheet() => new(
        new Fonts(
            new Font(new FontSize { Val = 11 }, new Color { Rgb = new HexBinaryValue { Value = "000000" } }, new FontName { Val = "Calibri" })),
        new Fills(
            new Fill(new PatternFill { PatternType = PatternValues.None }),
            new Fill(new PatternFill(new ForegroundColor { Rgb = new HexBinaryValue { Value = "FFFFFF" } }) { PatternType = PatternValues.Solid })),
        new Borders(
            new Border(new LeftBorder(), new RightBorder(), new TopBorder(), new BottomBorder(), new DiagonalBorder())),
        new CellFormats(
            new CellFormat { FontId = 0, FillId = 0, BorderId = 0, FormatId = 0 },
            new CellFormat { FontId = 0, FillId = 0, BorderId = 0, Alignment = new Alignment { WrapText = true }, ApplyAlignment = true }));
}
