using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Diagnostics;
using Wolf.Desktop.Services;
using Wolf.Dtos;

namespace Wolf.Desktop.Inqueries;

public class EmployeeTaskTypePaymentInquiry
{
    private readonly List<RequestDto> _requests;
    private readonly List<EmployeeDto> _employees;

    public EmployeeTaskTypePaymentInquiry(List<RequestDto> requests, List<EmployeeDto> employees)
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

            var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
            stylesPart.Stylesheet = CreateStylesheet();
            stylesPart.Stylesheet.Save();

            var sheets = document.WorkbookPart!.Workbook.AppendChild(new Sheets());
            uint sheetId = 1;

            foreach (var employee in _employees)
            {
                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());

                var sheetName = SanitizeSheetName(GetEmployeeName(employee), sheetId);
                sheets.Append(new Sheet
                {
                    Id = document.WorkbookPart.GetIdOfPart(worksheetPart),
                    SheetId = sheetId,
                    Name = sheetName
                });

                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>()!;
                AddHeaderRow(sheetData);
                SetColumnWidths(worksheetPart);
                PopulateEmployeeData(worksheetPart, sheetData, employee, cache);

                worksheetPart.Worksheet.Save();
                sheetId++;
            }

            workbookPart.Workbook.Save();
        }

        Process.Start(new ProcessStartInfo(outputPath) { UseShellExecute = true });
    }

    private void PopulateEmployeeData(WorksheetPart worksheetPart, SheetData sheetData, EmployeeDto employee, DataCacheService cache)
    {
        uint wrapStyle = 1;

        var employeeRequests = _requests.Where(req =>
        {
            var activities = cache.GetActivitiesForRequest(req.Requestid);
            return activities.Any(act =>
                cache.GetTasksForActivity(act.Activityid)
                    .Any(t => t.Executantid == employee.Employeeid && t.Executantpayment > 0));
        }).ToList();

        var requestNames = new List<string>();
        var requestNumbers = new List<string>();
        var allEmployeeTasks = new List<TaskDto>();

        foreach (var request in employeeRequests)
        {
            requestNames.Add(request.Requestname);
            requestNumbers.Add(request.Requestid.ToString());

            var activities = cache.GetActivitiesForRequest(request.Requestid);
            foreach (var act in activities)
            {
                var tasks = cache.GetTasksForActivity(act.Activityid)
                    .Where(t => t.Executantid == employee.Employeeid && t.Executantpayment > 0);
                allEmployeeTasks.AddRange(tasks);
            }
        }

        var taskTypeGroups = allEmployeeTasks
            .GroupBy(t => t.Tasktypeid)
            .Select(g =>
            {
                var tt = cache.GetTaskType(g.Key);
                return new
                {
                    TaskTypeName = tt?.Tasktypename ?? "",
                    TaskCount = g.Count(),
                    TotalPayment = g.Sum(t => t.Executantpayment)
                };
            })
            .OrderBy(g => g.TaskTypeName)
            .ToList();

        string allRequestNames = string.Join("; ", requestNames);
        int requestCount = employeeRequests.Count;
        string allRequestNums = string.Join("; ", requestNumbers);

        int dataRowCount = Math.Max(1, taskTypeGroups.Count);

        for (int i = 0; i < dataRowCount; i++)
        {
            uint rowIndex = (uint)(i + 2);
            var row = new Row { RowIndex = rowIndex };
            sheetData.Append(row);

            if (i == 0)
            {
                AddTextCell(row, 1, allRequestNames, wrapStyle);
                AddNumberCell(row, 2, requestCount.ToString(), wrapStyle);
                AddTextCell(row, 3, allRequestNums, wrapStyle);
            }
            else
            {
                AddTextCell(row, 1, "", wrapStyle);
                AddTextCell(row, 2, "", wrapStyle);
                AddTextCell(row, 3, "", wrapStyle);
            }

            if (i < taskTypeGroups.Count)
            {
                var g = taskTypeGroups[i];
                AddTextCell(row, 4, g.TaskTypeName, wrapStyle);
                AddNumberCell(row, 5, g.TaskCount.ToString(), wrapStyle);
                AddNumberCell(row, 6, g.TotalPayment.ToString("F2"), wrapStyle);
            }
            else
            {
                AddTextCell(row, 4, "", wrapStyle);
                AddTextCell(row, 5, "", wrapStyle);
                AddTextCell(row, 6, "", wrapStyle);
            }
        }

        if (dataRowCount > 1)
        {
            MergeCells(worksheetPart, 2, 1, (uint)(dataRowCount + 1));
            MergeCells(worksheetPart, 2, 2, (uint)(dataRowCount + 1));
            MergeCells(worksheetPart, 2, 3, (uint)(dataRowCount + 1));
        }
    }

    private static string GetEmployeeName(EmployeeDto e) =>
        string.Join(" ", new[] { e.Firstname, e.Secondname, e.Lastname }.Where(s => !string.IsNullOrEmpty(s)));

    private static string SanitizeSheetName(string name, uint sheetId)
    {
        if (string.IsNullOrEmpty(name)) return $"Employee_{sheetId}";
        foreach (var c in new[] { '\\', '/', '*', '[', ']', ':', '?' })
            name = name.Replace(c.ToString(), "");
        return name.Length > 31 ? name[..31] : name;
    }

    private static void AddHeaderRow(SheetData sheetData)
    {
        var row = new Row { RowIndex = 1 };
        sheetData.Append(row);
        AddTextCell(row, 1, "поръчки");
        AddTextCell(row, 2, "брой");
        AddTextCell(row, 3, "Номер на поръчки");
        AddTextCell(row, 4, "задача име");
        AddTextCell(row, 5, "Брой задачи");
        AddTextCell(row, 6, "сума");
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
        cols.Append(new Column { Min = 1, Max = 1, Width = 50, CustomWidth = true });
        cols.Append(new Column { Min = 2, Max = 2, Width = 10, CustomWidth = true });
        cols.Append(new Column { Min = 3, Max = 3, Width = 30, CustomWidth = true });
        cols.Append(new Column { Min = 4, Max = 4, Width = 30, CustomWidth = true });
        cols.Append(new Column { Min = 5, Max = 5, Width = 15, CustomWidth = true });
        cols.Append(new Column { Min = 6, Max = 6, Width = 15, CustomWidth = true });
        wsp.Worksheet.InsertAt(cols, 0);
    }

    private static Stylesheet CreateStylesheet() => new(
        new Fonts(
            new Font(new FontSize { Val = 11 }, new Color { Rgb = new HexBinaryValue { Value = "000000" } }, new FontName { Val = "Calibri" }),
            new Font(new Bold(), new FontSize { Val = 11 }, new Color { Rgb = new HexBinaryValue { Value = "000000" } }, new FontName { Val = "Calibri" })),
        new Fills(
            new Fill(new PatternFill { PatternType = PatternValues.None }),
            new Fill(new PatternFill(new ForegroundColor { Rgb = new HexBinaryValue { Value = "FFFFFF" } }) { PatternType = PatternValues.Solid })),
        new Borders(
            new Border(new LeftBorder(), new RightBorder(), new TopBorder(), new BottomBorder(), new DiagonalBorder())),
        new CellFormats(
            new CellFormat { FontId = 0, FillId = 0, BorderId = 0, FormatId = 0 },
            new CellFormat
            {
                FontId = 0, FillId = 0, BorderId = 0,
                Alignment = new Alignment { WrapText = true, Vertical = VerticalAlignmentValues.Top },
                ApplyAlignment = true
            }));
}
