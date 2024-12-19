using System.Text.Json;

namespace ReportHelper
{
    public class CompareDate
    {
        public static void Main(string[] args)
        {

            string rootDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../../Reports"));
            //pipelin result json file in this time
            string newDataPath = Path.Combine(rootDirectory, "new.json");
            //pipleline result json file last time
            string oldDataPath = Path.Combine(rootDirectory, "old.json");

            List<TResult4Json> newDataList = new List<TResult4Json>();
            List<TResult4Json> oldDataList = new List<TResult4Json>();

            if (File.Exists(newDataPath))
            {
                newDataList = JsonSerializer.Deserialize<List<TResult4Json>>(File.ReadAllText(newDataPath)) ?? new List<TResult4Json>();
            }
            if (File.Exists(oldDataPath))
            {
                oldDataList = JsonSerializer.Deserialize<List<TResult4Json>>(File.ReadAllText(oldDataPath)) ?? new List<TResult4Json>();
            }

            // Compare the two lists
            List<TResult4Json> differentList = new List<TResult4Json>();
            differentList = CompareLists(oldDataList, newDataList);

            // Save the different results to json and excel files
            string differentDataFileName = "DifferentReportResults.json";
            JsonHelper4Test.AddTestResult(differentList, differentDataFileName);

            string excelFileName = "ShowReportResults.xlsx";
            string differentSheetName = "DifferentSheet";
            ExcelHelper4Test.AddTestResult(differentList, excelFileName, differentSheetName);
        }
        public static List<TResult4Json> CompareLists(List<TResult4Json> oldDataList, List<TResult4Json> newDataList)
        {
            List<TResult4Json> differentList = new List<TResult4Json>();

            foreach (var newItem in newDataList)
            {
                var matchedOldItem = oldDataList.FirstOrDefault(oldItem =>
                        oldItem.TestCase == newItem.TestCase &&
                        oldItem.ErrorInfo == newItem.ErrorInfo &&
                        oldItem.ErrorLink == newItem.ErrorLink
                    );
                //  new TResult is diffrent
                if (matchedOldItem == null)
                {
                    differentList.Add(newItem);
                    continue;
                }
                // new TResult is same , but locations of error is diffrent
                List<string> differentLocationsList = CompareOfLocations(matchedOldItem.LocationsOfErrors, newItem.LocationsOfErrors);
                if (differentLocationsList.Count > 0)
                {
                    newItem.LocationsOfErrors = differentLocationsList;
                    differentList.Add(newItem);
                    continue;
                }
            }
            return differentList;
        }
        public static List<string> CompareOfLocations(List<string> oldLocationsList, List<string> newLocationsList)
        {
            List<string> differentLocationsList = new List<string>();
            if (newLocationsList == null && oldLocationsList == null) { return differentLocationsList; }

            // 之前的if判断已经排除了两个都为null的情况
            if (oldLocationsList == null || newLocationsList == null) { return differentLocationsList; }

            foreach (var location in newLocationsList)
            {
                string str = location.Contains(".") ? location.Substring(location.IndexOf(".") + 1) : location;
                // If new location is not in old locations, they are different
                if (!oldLocationsList.Contains(location))
                {
                    differentLocationsList.Add(location);
                }
            }
            return differentLocationsList;
        }
    }
}