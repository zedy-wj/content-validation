using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Collections.Concurrent;
using UtilityLibraries;

public class ExcelHelper4Test
{
    private static string filePath;
    private static readonly object LockObj = new object();

    static ExcelHelper4Test()
    {
        // Define the file path for the Excel file
        string filePath = "TestData.xlsx";
        string rootDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
        ExcelHelper4Test.filePath = Path.Combine(rootDirectory, filePath);
        Console.WriteLine(ExcelHelper4Test.filePath);

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
                    row.CreateCell(1).SetCellValue("Test cases");
                    row.CreateCell(2).SetCellValue("Error Info");
                    row.CreateCell(3).SetCellValue("Number of occurrences");
                    row.CreateCell(4).SetCellValue("Location of errors");
                    row.CreateCell(5).SetCellValue("Error Link");
                    row.CreateCell(6).SetCellValue("Notes");

                    workbook.Write(fs);
                    workbook.Close();
                }
            }
        }
    }

    public static void AddTestResult(ConcurrentQueue<TResult> TResults)
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

                foreach (var res in TResults)
                {
                    // Create a new row and populate cells with test result data
                    IRow row = sheet.CreateRow(sheet.LastRowNum + 1);
                    row.CreateCell(0).SetCellValue(sheet.LastRowNum);
                    row.CreateCell(1).SetCellValue(res.TestCase);
                    row.CreateCell(2).SetCellValue(res.ErrorInfo);
                    row.CreateCell(3).SetCellValue(res.NumberOfOccurrences);

                    ICell cell5 = row.CreateCell(4);
                    cell5.SetCellValue(string.Join("\n", res.LocationsOfErrors));
                    cell5.CellStyle = cellStyle; 

                    row.CreateCell(5).SetCellValue(res.ErrorLink);
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
