using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Diagnostics;
using Wolf.Desktop.Services;
using Wolf.Dtos;

namespace Wolf.Desktop.Inqueries;

public class AllTasksInquiry
{
    private readonly List<RequestDto> _requests;
    private readonly List<EmployeeDto> _employees;

    public AllTasksInquiry(List<RequestDto> requests, List<EmployeeDto> employees)
    {
        _requests = requests;
        _employees = employees;
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
                Name = "Inquiries"
            });

            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>()!;
            AddHeaderRow(sheetData);
            SetColumnWidths(worksheetPart);

            var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
            stylesPart.Stylesheet = CreateStylesheet();
            stylesPart.Stylesheet.Save();

            uint wrapStyle = 1;
            uint rowIndex = 2;

            foreach (var employee in _employees)
            {
                var empName = GetEmployeeName(employee);
                // Find requests that have activities with tasks assigned to this employee
                var employeeRequests = _requests.Where(req =>
                {
                    var activities = cache.GetActivitiesForRequest(req.Requestid);
                    return activities.Any(act =>
                    {
                        var tasks = cache.GetTasksForActivity(act.Activityid);
                        return tasks.Any(t => t.Executantid == employee.Employeeid);
                    });
                }).ToList();

                int employeeRowSpan = 0;

                foreach (var request in employeeRequests)
                {
                    var activities = cache.GetActivitiesForRequest(request.Requestid)
                        .Where(act => cache.GetTasksForActivity(act.Activityid)
                            .Any(t => t.Executantid == employee.Employeeid))
                        .ToList();

                    int requestRowSpan = 0;

                    foreach (var activity in activities)
                    {
                        var actType = cache.GetActivityType(activity.Activitytypeid);
                        var tasks = cache.GetTasksForActivity(activity.Activityid)
                            .Where(t => t.Executantid == employee.Employeeid)
                            .ToList();

                        int activityRowSpan = 0;

                        foreach (var task in tasks)
                        {
                            var taskType = cache.GetTaskType(task.Tasktypeid);
                            var row = new Row { RowIndex = rowIndex, CustomHeight = false };
                            sheetData.Append(row);

                            if (employeeRowSpan == 0)
                                AddTextCell(row, 1, empName, wrapStyle);
                            if (requestRowSpan == 0)
                            {
                                AddTextCell(row, 2, request.Requestname, wrapStyle);
                                AddNumberCell(row, 3, request.Price.ToString(), wrapStyle);
                                AddTextCell(row, 4, request.Path ?? "", wrapStyle);
                            }
                            if (activityRowSpan == 0)
                            {
                                AddTextCell(row, 5, actType?.Activitytypename ?? "", wrapStyle);
                                AddNumberCell(row, 6, activity.Employeepayment.ToString(), wrapStyle);
                            }

                            AddTextCell(row, 7, taskType?.Tasktypename ?? "", wrapStyle);
                            AddNumberCell(row, 8, task.Executantpayment.ToString(), wrapStyle);
                            AddTextCell(row, 9, (task.Duration.Hour + task.Duration.Minute / 60.0).ToString("F2"), wrapStyle);

                            rowIndex++;
                            requestRowSpan++;
                            activityRowSpan++;
                            employeeRowSpan++;
                        }

                        if (activityRowSpan > 1)
                        {
                            MergeCells(worksheetPart, rowIndex - (uint)activityRowSpan, 5, rowIndex - 1);
                            MergeCells(worksheetPart, rowIndex - (uint)activityRowSpan, 6, rowIndex - 1);
                        }
                    }

                    if (requestRowSpan > 1)
                    {
                        MergeCells(worksheetPart, rowIndex - (uint)requestRowSpan, 2, rowIndex - 1);
                        MergeCells(worksheetPart, rowIndex - (uint)requestRowSpan, 3, rowIndex - 1);
                        MergeCells(worksheetPart, rowIndex - (uint)requestRowSpan, 4, rowIndex - 1);
                    }
                }

                if (employeeRowSpan > 1)
                    MergeCells(worksheetPart, rowIndex - (uint)employeeRowSpan, 1, rowIndex - 1);
            }

            worksheetPart.Worksheet.Save();
        }

        Process.Start(new ProcessStartInfo(outputPath) { UseShellExecute = true });
    }

    private static string GetEmployeeName(EmployeeDto e) =>
        string.Join(" ", new[] { e.Firstname, e.Secondname, e.Lastname }.Where(s => !string.IsNullOrEmpty(s)));

    private void AddHeaderRow(SheetData sheetData)
    {
        var row = new Row { RowIndex = 1 };
        sheetData.Append(row);
        AddTextCell(row, 1, "Служител");
        AddTextCell(row, 2, "Поръчка");
        AddTextCell(row, 3, "Поръчка цена");
        AddTextCell(row, 4, "Път");
        AddTextCell(row, 5, "Дейност");
        AddTextCell(row, 6, "Хонорар гл. Изпълнител");
        AddTextCell(row, 7, "Задачи");
        AddTextCell(row, 8, "Хонорар изпълнител");
        AddTextCell(row, 9, "Времетраене");
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

    private static void MergeCells(WorksheetPart wsp, uint startRow, int col, uint endRow)
    {
        if (startRow == endRow) return;
        var mc = wsp.Worksheet.Elements<MergeCells>().FirstOrDefault();
        if (mc is null) { mc = new MergeCells(); wsp.Worksheet.InsertAfter(mc, wsp.Worksheet.Elements<SheetData>().First()); }
        mc.Append(new MergeCell { Reference = new StringValue($"{GetCellRef(col, (int)startRow)}:{GetCellRef(col, (int)endRow)}") });
    }

    private static void SetColumnWidths(WorksheetPart wsp)
    {
        var cols = new Columns();
        cols.Append(new Column { Min = 1, Max = 1, Width = 25, CustomWidth = true });
        cols.Append(new Column { Min = 2, Max = 2, Width = 25, CustomWidth = true });
        cols.Append(new Column { Min = 3, Max = 3, Width = 20, CustomWidth = true });
        cols.Append(new Column { Min = 4, Max = 4, Width = 25, CustomWidth = true });
        cols.Append(new Column { Min = 5, Max = 5, Width = 30, CustomWidth = true });
        cols.Append(new Column { Min = 6, Max = 6, Width = 25, CustomWidth = true });
        cols.Append(new Column { Min = 7, Max = 7, Width = 25, CustomWidth = true });
        cols.Append(new Column { Min = 8, Max = 8, Width = 25, CustomWidth = true });
        cols.Append(new Column { Min = 9, Max = 9, Width = 20, CustomWidth = true });
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
