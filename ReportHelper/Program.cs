using System.Text.Json;

namespace ReportHelper
{
    public class CompareDate
    {
        public static void Main(string[] args)
        {

            string rootDirectory = ConstData.ReportsDirectory;

            //pipelin result json file in this time
            string newDataPath = Path.Combine(rootDirectory, ConstData.TotalIssuesJsonFileName);
            //pipleline result json file last time
            string? oldDataPath = ConstData.LastPipelineDiffIssuesJsonFileName;

            List<TResult4Json> newDataList = new List<TResult4Json>();
            List<TResult4Json> oldDataList = new List<TResult4Json>();

            if (File.Exists(newDataPath))
            {
                newDataList = JsonSerializer.Deserialize<List<TResult4Json>>(File.ReadAllText(newDataPath)) ?? new List<TResult4Json>();
            }
            if (oldDataList != null && File.Exists(oldDataPath))
            {
                oldDataList = JsonSerializer.Deserialize<List<TResult4Json>>(File.ReadAllText(oldDataPath)) ?? new List<TResult4Json>();
            }

            // Compare the two lists
            List<TResult4Json> differentList = new List<TResult4Json>();
            differentList = CompareLists(oldDataList, newDataList);

            // Save the different results to json and excel files
            string differentDataFileName = ConstData.DiffIssuesJsonFileName;
            JsonHelper4Test.AddTestResult(differentList, differentDataFileName);

            string excelFileName = ConstData.DiffIssuesExcelFileName;
            string differentSheetName = "DiffSheet";
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

            if (oldLocationsList == null || newLocationsList == null) { return differentLocationsList; }

            var processedOldLocationsList = oldLocationsList
                    .Select(location => location.Contains(".") ? location.Substring(location.IndexOf(".") + 1) : location)
                    .ToList();

            int count = 0;

            foreach (var location in newLocationsList)
            {
                string str = location.Contains(".") ? location.Substring(location.IndexOf(".") + 1) : location;
                // If new location is not in old locations, they are different
                if (!processedOldLocationsList.Contains(str))
                {
                    differentLocationsList.Add($"{++count}.{str}");
                }
            }
            return differentLocationsList;
        }
    }
}