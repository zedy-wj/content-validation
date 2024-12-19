using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReportHelper
{
    public class CompareDate
    {
        public static void Main(string[] args)
        {
            // 获取数据
            //pipelin result json file in this time
            string rootDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../../Reports"));

            string newDataPath = Path.Combine(rootDirectory, "ReportResults.json");
            //pipleline result json file last time
            string oldDataPath = Path.Combine(rootDirectory, "OldReportResultsData.json");

            List<TResult4Json> newDataList = new List<TResult4Json>();
            List<TResult4Json> oldDataList = new List<TResult4Json>();

            if (File.Exists(newDataPath))
            {
                Console.WriteLine("有issue");
                newDataList = JsonSerializer.Deserialize<List<TResult4Json>>(File.ReadAllText(newDataPath)) ?? new List<TResult4Json>();
            }
            else
            {
                Console.WriteLine("没有newDataList,newDataList使用空数据,showReportResults.xlsx文件中same和diff没有数据");
            }

            if (File.Exists(oldDataPath))
            {
                Console.WriteLine("需要diff");
                oldDataList = JsonSerializer.Deserialize<List<TResult4Json>>(File.ReadAllText(oldDataPath)) ?? new List<TResult4Json>();
            }
            else
            {
                Console.WriteLine("没有oldDataList,第一次跑pipeline,使用空数据,showReportResults.xlsx文件中same没有数据,different有数据");
            }

            // 执行算法
            List<TResult4Json> differentList = new List<TResult4Json>();
            List<TResult4Json> sameList = new List<TResult4Json>();
            (sameList, differentList) = CompareLists(oldDataList, newDataList);

            // 保存数据
            string sameDataFileName = "SameReportResults.json";
            string differentDataFileName = "DifferentReportResults.json";
            JsonHelper4Test.AddTestResult(sameList, sameDataFileName);
            JsonHelper4Test.AddTestResult(differentList, differentDataFileName);

            string excelFileName = "ShowReportResults.xlsx";
            string sameSheetName = "SameSheet";
            string differentSheetName = "DifferentSheet";
            ExcelHelper4Test.AddTestResult(sameList, excelFileName, sameSheetName);
            ExcelHelper4Test.AddTestResult(differentList, excelFileName, differentSheetName);
        }

        public static (List<TResult4Json> sameList, List<TResult4Json> differentList) CompareLists(List<TResult4Json> oldDataList, List<TResult4Json> newDataList)
        {
            List<TResult4Json> sameList = new List<TResult4Json>();
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
                if (!isSameOfLocations(matchedOldItem.LocationsOfErrors, newItem.LocationsOfErrors))
                {
                    differentList.Add(newItem);
                    continue;
                }

                // new TResult is same
                sameList.Add(newItem);
            }
            return (sameList, differentList);
        }


        public static bool isSameOfLocations(List<string> oldLocationsList, List<string> newLocationsList)
        {
            if (newLocationsList == null && oldLocationsList == null) { return true; }

            // 之前的if判断已经排除了两个都为null的情况
            if (oldLocationsList == null || newLocationsList == null) { return false; }
            // ??????????????
            if (oldLocationsList.Count < newLocationsList.Count) { return false; }

            foreach (var location in newLocationsList)
            {
                string str = location.Contains(".") ? location.Substring(location.IndexOf(".") + 1) : location;
                // If new location is not in old locations, they are different
                if (!oldLocationsList.Contains(str)) { return false; }
            }

            return true;
        }

    }




}