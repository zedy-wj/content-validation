using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Collections.Concurrent;
using System.Text.Json;
using UtilityLibraries;
namespace ReportHelper;
public class ExcelHelper4Test
{
    private static readonly object LockObj = new object();

    // Initialize the Excel file if it doesn't exist
    public static string Init(string fileName, string sheetName)
    {
        // Define the root directory for the Excel file
        string rootDirectory = ConstData.ReportsDirectory;
        {
            Directory.CreateDirectory(rootDirectory);
        }

        string localFilePath = Path.Combine(rootDirectory, fileName);

        IWorkbook workbook;

        // Check if file exists
        if (File.Exists(localFilePath))
        {
            // Load existing workbook
            using (var fs = new FileStream(localFilePath, FileMode.Open, FileAccess.ReadWrite))
            {
                workbook = WorkbookFactory.Create(fs);
            }
        }
        else
        {
            // Create new workbook
            workbook = new XSSFWorkbook();
        }

        // Check if the sheet exists
        ISheet sheet = workbook.GetSheet(sheetName);
        if (sheet == null)
        {
            // Create new sheet
            sheet = workbook.CreateSheet(sheetName);

            // Create header row with column names
            IRow row = sheet.CreateRow(0);
            row.CreateCell(0).SetCellValue("NO.");
            row.CreateCell(1).SetCellValue("Error Info");
            row.CreateCell(2).SetCellValue("Number of occurrences");
            row.CreateCell(3).SetCellValue("Location of errors");
            row.CreateCell(4).SetCellValue("Error Link");
            row.CreateCell(5).SetCellValue("Test cases");
            row.CreateCell(6).SetCellValue("Notes");
        }

        // Save workbook
        using (var fs = new FileStream(localFilePath, FileMode.Create, FileAccess.Write))
        {
            workbook.Write(fs);
        }

        workbook.Close();

        return localFilePath;
    }
    public static void AddTestResult(ConcurrentQueue<TResult> testResults, string fileName, string sheetName)
    {
        lock (LockObj)
        {
            string localFilePath = Init(fileName, sheetName);
            using (var fs = new FileStream(localFilePath, FileMode.Open, FileAccess.ReadWrite))
            {
                IWorkbook workbook = new XSSFWorkbook(fs);
                ISheet sheet = workbook.GetSheet(sheetName);

                // Create a cell style to enable text wrapping
                ICellStyle cellStyle = workbook.CreateCellStyle();
                cellStyle.WrapText = true;

                // Create a cell style for hyperlinks
                ICellStyle hlinkStyle = workbook.CreateCellStyle();
                IFont hlinkFont = workbook.CreateFont();
                hlinkFont.Underline = FontUnderlineType.Single;
                hlinkFont.FontName = "Aptos Narrow";
                hlinkFont.Color = IndexedColors.Blue.Index;
                hlinkStyle.SetFont(hlinkFont);

                foreach (var res in testResults)
                {
                    // Create a new row and populate cells with test result data
                    IRow row = sheet.CreateRow(sheet.LastRowNum + 1);

                    row.CreateCell(0).SetCellValue(sheet.LastRowNum);
                    row.CreateCell(1).SetCellValue(res.ErrorInfo);
                    row.CreateCell(2).SetCellValue(res.NumberOfOccurrences);

                    ICell cell3 = row.CreateCell(3);
                    cell3.SetCellValue(string.Join("\n", res.LocationsOfErrors ?? new List<string>()));
                    cell3.CellStyle = cellStyle;

                    ICell cell4 = row.CreateCell(4);
                    cell4.SetCellValue(res.ErrorLink);
                    IHyperlink link = workbook.GetCreationHelper().CreateHyperlink(HyperlinkType.Url);
                    link.Address = res.ErrorLink;
                    cell4.Hyperlink = link;
                    cell4.CellStyle = hlinkStyle;

                    row.CreateCell(5).SetCellValue(res.TestCase);
                    row.CreateCell(6).SetCellValue(res.AdditionalNotes?.ToString());
                }

                // Automatically adjust column widths to fit content
                for (int col = 0; col < sheet.GetRow(0).LastCellNum; col++)
                {
                    sheet.AutoSizeColumn(col);
                }

                // Save the updated workbook back to the file
                using (var outFs = new FileStream(localFilePath, FileMode.Create, FileAccess.Write))
                {
                    workbook.Write(outFs);
                    workbook.Close();
                }
            }
        }
    }


    public static void AddTestResult(List<TResult4Json> testResults, string fileName, string sheetName)
    {
        lock (LockObj)
        {
            string localFilePath = Init(fileName, sheetName);
            using (var fs = new FileStream(localFilePath, FileMode.Open, FileAccess.ReadWrite))
            {
                IWorkbook workbook = new XSSFWorkbook(fs);
                ISheet sheet = workbook.GetSheet(sheetName);

                // Create a cell style to enable text wrapping
                ICellStyle cellStyle = workbook.CreateCellStyle();
                cellStyle.WrapText = true;

                // Create a cell style for hyperlinks
                ICellStyle hlinkStyle = workbook.CreateCellStyle();
                IFont hlinkFont = workbook.CreateFont();
                hlinkFont.Underline = FontUnderlineType.Single;
                hlinkFont.FontName = "Aptos Narrow";
                hlinkFont.Color = IndexedColors.Blue.Index;
                hlinkStyle.SetFont(hlinkFont);

                foreach (var res in testResults)
                {
                    // Create a new row and populate cells with test result data
                    IRow row = sheet.CreateRow(sheet.LastRowNum + 1);

                    row.CreateCell(0).SetCellValue(sheet.LastRowNum);
                    row.CreateCell(1).SetCellValue(res.ErrorInfo);
                    row.CreateCell(2).SetCellValue(res.NumberOfOccurrences ?? 0);

                    ICell cell3 = row.CreateCell(3);
                    cell3.SetCellValue(string.Join("\n", res.LocationsOfErrors ?? new List<string>()));
                    cell3.CellStyle = cellStyle;

                    ICell cell4 = row.CreateCell(4);
                    cell4.SetCellValue(res.ErrorLink);
                    IHyperlink link = workbook.GetCreationHelper().CreateHyperlink(HyperlinkType.Url);
                    link.Address = res.ErrorLink;
                    cell4.Hyperlink = link;
                    cell4.CellStyle = hlinkStyle;

                    row.CreateCell(5).SetCellValue(res.TestCase);
                    row.CreateCell(6).SetCellValue(res.AdditionalNotes?.ToString());
                }

                // Automatically adjust column widths to fit content
                for (int col = 0; col < sheet.GetRow(0).LastCellNum; col++)
                {
                    sheet.AutoSizeColumn(col);
                }

                // Save the updated workbook back to the file
                using (var outFs = new FileStream(localFilePath, FileMode.Create, FileAccess.Write))
                {
                    workbook.Write(outFs);
                    workbook.Close();
                }
            }
        }
    }



}
public class JsonHelper4Test
{
    private static readonly object LockObj = new object();

    // Initialize the Json file if it doesn't exist
    public static string Init(string filePath)
    {

        // Define the file path for the Excel file
        string rootDirectory = ConstData.ReportsDirectory;
        if (!Directory.Exists(rootDirectory))
        {
            Directory.CreateDirectory(rootDirectory);
        }
        string localFilePath = Path.Combine(rootDirectory, filePath);
        if (!File.Exists(localFilePath))
        {
            var emptyList = new List<TResult4Json>();
            string jsonString = JsonSerializer.Serialize(emptyList, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(localFilePath, jsonString);
        }

        return localFilePath;

    }

    public static void AddTestResult(ConcurrentQueue<TResult> testResults, string fileName)
    {

        lock (LockObj)
        {
            string localFilePath = Init(fileName);
            string jsonString = File.ReadAllText(localFilePath);
            List<TResult4Json> jsonList = JsonSerializer.Deserialize<List<TResult4Json>>(jsonString)!;
            int count = jsonList.Count;

            foreach (var res in testResults)
            {
                TResult4Json result = new TResult4Json
                {
                    Number = ++count,
                    ErrorInfo = res.ErrorInfo,
                    NumberOfOccurrences = res.NumberOfOccurrences,
                    LocationsOfErrors = res.LocationsOfErrors,
                    ErrorLink = res.ErrorLink,
                    TestCase = res.TestCase,
                    AdditionalNotes = res.AdditionalNotes
                };

                jsonList.Add(result);
            }

            var options = new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            };

            File.WriteAllText(localFilePath, JsonSerializer.Serialize(jsonList, options));
        }
    }

    public static void AddTestResult(List<TResult4Json> testResults, string fileName)
    {
        lock (LockObj)
        {
            string localFilePath = Init(fileName);
            string jsonString = File.ReadAllText(localFilePath);
            List<TResult4Json> jsonList = JsonSerializer.Deserialize<List<TResult4Json>>(jsonString)!;
            int count = jsonList.Count;

            foreach (var res in testResults)
            {
                TResult4Json result = new TResult4Json
                {
                    Number = ++count,
                    ErrorInfo = res.ErrorInfo,
                    NumberOfOccurrences = res.NumberOfOccurrences,
                    LocationsOfErrors = res.LocationsOfErrors,
                    ErrorLink = res.ErrorLink,
                    TestCase = res.TestCase,
                    AdditionalNotes = res.AdditionalNotes
                };

                jsonList.Add(result);
            }

            var options = new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            };

            File.WriteAllText(localFilePath, JsonSerializer.Serialize(jsonList, options));
        }
    }
}

public class GithubHelper
{
    public static string FormatToMarkdown(List<TResult4Json> list)
    {

        string result = "";

        for (int i = 0; i < list.Count; i++)
        {
            var item = list[i];

            result += $"{i + 1}.\n";

            result += $"**ErrorInfo**: {item.ErrorInfo}\n";
            result += $"**ErrorLink**: {item.ErrorLink}\n";

            if (item.LocationsOfErrors != null && item.LocationsOfErrors.Count > 0)
            {
                result += $"**ErrorLocation**:\n";
                foreach (var location in item.LocationsOfErrors)
                {
                    string str = location.Contains(".") ? location.Substring(location.IndexOf(".") + 1) : location;
                    str = " -" + str;
                    result += $"{str}\n";
                }
                result += "\n";
            }


            if (!string.IsNullOrWhiteSpace(item.Note))
            {
                result += $"**Note**: {item.Note}\n";
            }

            result += $"\n";
        }

        result = result.Replace("\n", "\\n");
        result = result.Replace("\"", "\\\"");
        return result;
    }

    public static List<TResult4Json> DeduplicateList(List<TResult4Json> differentList)
    {
        var deduplicateList = differentList
            .GroupBy(item => item.ErrorInfo)
            .SelectMany(group =>
            {
                if (group.Count() > 3)
                {
                    var first = group.First();
                    string note = $"{first.ErrorInfo} - this type of issues appears {group.Count()} times, currently only one is shown here. For more details, please click on the excel download link below to view.";
                    first.Note = note;
                    return [first];
                }
                else
                {
                    return group;
                }
            })
            .ToList();

        return deduplicateList;
    }
}

