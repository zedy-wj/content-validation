using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace ExcelCompare
{
    public class ErrorData
    {
        [JsonProperty("NO.")]
        public int NO { get; set; }

        [JsonProperty("ErrorInfo")]
        public string ErrorInfo { get; set; }

        [JsonProperty("NumberOfOccurrences")]
        public int NumberOfOccurrences { get; set; }

        [JsonProperty("LocationsOfErrors")]
        public List<string> LocationsOfErrors { get; set; }

        [JsonProperty("ErrorLink")]
        public string ErrorLink { get; set; }

        [JsonProperty("TestCase")]
        public string TestCase { get; set; }

        [JsonProperty("AdditionalNotes")]
        public string AdditionalNotes { get; set; }
    }
    public class CompareJson
    {
        static void Main(string[] args)
        {
            //pipelin result json file in this time
            string originPath = "../../../new.json";
            //pipleline result json file last time
            string comparePath = "../../../old.json";
            //compare result 
            string resultPathDff = "../../../ResultListDff.json";
            string resultPathSame = "../../../ResultListSame.json";

            Console.WriteLine("Compare Started！");
            try
            {
                List<ErrorData> newData = JsonConvert.DeserializeObject<List<ErrorData>>(File.ReadAllText(originPath));
                List<ErrorData> compareData = JsonConvert.DeserializeObject<List<ErrorData>>(File.ReadAllText(comparePath));

                List<ErrorData> differences = new List<ErrorData>();
                List<ErrorData> sames = new List<ErrorData>();

                Console.WriteLine("JSON File Read Finished！");

                foreach (var NewItem in newData)
                {
                    //select data where these result same as testcase && errorinfo && errolink
                    //Only if all three conditions are met, the rub can be compared in the next step
                    var matchedItems = compareData
                        .Where(OldItem =>
                            OldItem.TestCase == NewItem.TestCase &&
                            OldItem.ErrorInfo == NewItem.ErrorInfo &&
                            OldItem.ErrorLink == NewItem.ErrorLink)
                        .ToList();

                    //if not, this is a new issue, stored it.
                    if (!matchedItems.Any())
                    {
                        differences.Add(NewItem);
                        continue;
                    }

                    //justify list locationof error
                    //when location of error diffrent,we need check it,and restore new condition.
                    //bool isDifferent = false;
                    List<string> unmatchedLocations = new List<string>();
                    List<string> matchedLocations = new List<string>();

                    if (NewItem.LocationsOfErrors != null)
                    {
                        //check over all data: 1 -> n;
                        foreach (var location in NewItem.LocationsOfErrors)
                        {
                            string trimmedLocation = ExtractRelevantPart(location);

                            // 检查是否存在匹配项
                            bool existsInAnyMatch = matchedItems.Any(match =>
                                match.LocationsOfErrors != null &&
                                match.LocationsOfErrors.Any(matchLocation =>
                                    ExtractRelevantPart(matchLocation) == trimmedLocation
                                )
                            );

                            //save diffrence in a new list
                            if (existsInAnyMatch)
                            {
                                matchedLocations.Add(location);
                            }
                            else
                            {
                                unmatchedLocations.Add(location);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Warning: LocationsOfErrors in NewItem (NO: {NewItem.NO}) is null.");
                    }

                    //when checkout diffrence, we will restore all new error, which will include all or `ErrorData`
                    if (unmatchedLocations.Count > 0)
                    {
                        differences.Add(new ErrorData
                        {
                            NO = NewItem.NO,
                            ErrorInfo = NewItem.ErrorInfo,
                            NumberOfOccurrences = NewItem.NumberOfOccurrences,
                            LocationsOfErrors = unmatchedLocations,
                            ErrorLink = NewItem.ErrorLink,
                            TestCase = NewItem.TestCase
                        });
                        Console.WriteLine(differences.ToString());
                    }

                    // Add matched locations to sames
                    if (matchedLocations.Count > 0)
                    {
                        sames.Add(new ErrorData
                        {
                            NO = NewItem.NO,
                            ErrorInfo = NewItem.ErrorInfo,
                            NumberOfOccurrences = NewItem.NumberOfOccurrences,
                            LocationsOfErrors = matchedLocations,
                            ErrorLink = NewItem.ErrorLink,
                            TestCase = NewItem.TestCase
                        });
                    }
                }
                Console.WriteLine("Compare Finished！");
                Console.Write(differences.Count);
                Console.Write(sames.Count);

                File.WriteAllText(resultPathSame, JsonConvert.SerializeObject(sames, Formatting.Indented));
                Console.WriteLine($"Sames file has been saved in {resultPathSame}");

                File.WriteAllText(resultPathDff, JsonConvert.SerializeObject(differences, Formatting.Indented));
                Console.WriteLine($"Differences file has been saved in {resultPathDff}");



                //convert json to xlsx
                string file_name = "../../../result.xlsx";
                ConvertJsonToExcel(resultPathSame, resultPathDff, file_name, "original_data", "capmpare_result");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
        private static string ExtractRelevantPart(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            // find first A character
            int startIndex = input.IndexOf('A');
            if (startIndex >= 0)
            {
                return input.Substring(startIndex).Trim();
            }

            //if faile, return
            return input;
        }

        /// <summary>
        /// To convert json result to xlsx file
        /// </summary>
        /// <param name="jsonFilePath">pipeline result</param>
        /// <param name="compare_jsonFilePath">diffrent result json file</param>
        /// <param name="excelFileName">excel name</param>
        /// <param name="sheetName1">sheet1 name</param>
        /// <param name="sheetName2">sheet2 name</param>
        static void ConvertJsonToExcel(string jsonFilePath, string compare_jsonFilePath, string excelFileName, string sheetName1, string sheetName2)
        {
            // Read Json
            //to read 
            var jsonData = File.ReadAllText(jsonFilePath);
            List<ErrorData> errorList = JsonConvert.DeserializeObject<List<ErrorData>>(jsonData);

            var compareJsonData = File.ReadAllText(compare_jsonFilePath);
            List<ErrorData> compareErrorList = JsonConvert.DeserializeObject<List<ErrorData>>(compareJsonData);

            // Create Excel Table
            IWorkbook workbook = new XSSFWorkbook();

            // Pipe Line Original Data
            ISheet sheet1 = workbook.CreateSheet(sheetName1);
            WriteDataToSheet(sheet1, errorList);

            if (compareErrorList.Count != 0)
            {
                // Compared Json Result File
                ISheet sheet2 = workbook.CreateSheet(sheetName2);
                WriteDataToSheet(sheet2, compareErrorList);
            }

            // Save in aim path
            using (FileStream fileStream = new FileStream(excelFileName, FileMode.Create, FileAccess.Write))
            {
                workbook.Write(fileStream);
            }
        }
        static void WriteDataToSheet(ISheet sheet, List<ErrorData> dataList)
        {
            // Excel Header
            IRow headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("NO.");
            headerRow.CreateCell(1).SetCellValue("Error Info");
            headerRow.CreateCell(2).SetCellValue("Number of occurrences");
            headerRow.CreateCell(3).SetCellValue("Location of errors");
            headerRow.CreateCell(4).SetCellValue("Error Link");
            headerRow.CreateCell(5).SetCellValue("Test cases");

            // Write Rows Data
            for (int i = 0; i < dataList.Count; i++)
            {
                var data = dataList[i];
                IRow row = sheet.CreateRow(i + 1);
                row.CreateCell(0).SetCellValue(data.NO);
                row.CreateCell(1).SetCellValue(data.ErrorInfo);
                row.CreateCell(2).SetCellValue(data.NumberOfOccurrences);
                string locationErrors = data.LocationsOfErrors != null
                    ? string.Join("\n", data.LocationsOfErrors)
                    : string.Empty;
                row.CreateCell(3).SetCellValue(locationErrors);
                //row.CreateCell(3).SetCellValue(data.LocationOfErrors);
                row.CreateCell(4).SetCellValue(data.ErrorLink);
                row.CreateCell(5).SetCellValue(data.TestCase);
            }
        }
    }
}
