using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using UtilityLibraries;

namespace ContentValidation.Test
{
    public class ExcelHelper4Test
    {
        private static string filePath;
        private static readonly object LockObj = new object();


        static ExcelHelper4Test()
        {
            // Define the file path for the Excel file
            string filePath = "ReportResults.xlsx";
            string rootDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../../Reports"));
            if (!Directory.Exists(rootDirectory))
            {
                Directory.CreateDirectory(rootDirectory);
            }
            ExcelHelper4Test.filePath = Path.Combine(rootDirectory, filePath);

            Init();
        }

        // Initialize the Excel file if it doesn't exist
        public static void Init()
        {
            lock (LockObj)
            {
                if (!File.Exists(filePath))
                {
                    using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        IWorkbook workbook = new XSSFWorkbook();
                        ISheet sheet = workbook.CreateSheet("TestSheet1");

                        // Create header row with column names
                        IRow row = sheet.CreateRow(0);
                        row.CreateCell(0).SetCellValue("NO.");
                        row.CreateCell(1).SetCellValue("Error Info");
                        row.CreateCell(2).SetCellValue("Number of occurrences");
                        row.CreateCell(3).SetCellValue("Location of errors");
                        row.CreateCell(4).SetCellValue("Error Link");
                        row.CreateCell(5).SetCellValue("Test cases");
                        row.CreateCell(6).SetCellValue("Notes");

                        workbook.Write(fs);
                        workbook.Close();
                    }
                }
            }
        }

        public static void AddTestResult(ConcurrentQueue<TResult> testResults)
        {
            lock (LockObj)
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
                {
                    IWorkbook workbook = new XSSFWorkbook(fs);
                    ISheet sheet = workbook.GetSheetAt(0);

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
                        cell3.SetCellValue(string.Join("\n", res.LocationsOfErrors));
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
                    using (var outFs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
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
        private static string filePath;
        private static readonly object LockObj = new object();
        static JsonHelper4Test()
        {
            // Define the file path for the Excel file
            string filePath = "ReportResults.json";
            string rootDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../../Reports"));
            if (!Directory.Exists(rootDirectory))
            {
                Directory.CreateDirectory(rootDirectory);
            }
            JsonHelper4Test.filePath = Path.Combine(rootDirectory, filePath);

            Init();
        }


        // Initialize the Json file if it doesn't exist
        public static void Init()
        {
            lock (LockObj)
            {
                if (!File.Exists(filePath))
                {
                    var emptyList = new List<TResult4Json>();
                    string jsonString = JsonSerializer.Serialize(emptyList, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(filePath, jsonString);
                }
            }
        }

        public static void AddTestResult(ConcurrentQueue<TResult> testResults)
        {

            lock (LockObj)
            {

                string jsonString = File.ReadAllText(filePath);
                List<TResult4Json> jsonList = JsonSerializer.Deserialize<List<TResult4Json>>(jsonString);
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

                File.WriteAllText(filePath, JsonSerializer.Serialize(jsonList, options));
            }
        }
    }


    public class TResult4Json
    {
        [JsonPropertyName("NO.")]
        public int? Number { get; set; }
        public string? ErrorInfo { get; set; }
        public int? NumberOfOccurrences { get; set; }
        public List<string>? LocationsOfErrors { get; set; }
        public string? ErrorLink { get; set; }
        public string? TestCase { get; set; }
        public object? AdditionalNotes { get; set; }
    }

}
