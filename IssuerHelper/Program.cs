using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IssuerHelper
{
    public class Issuer
    {
        static void Main(string[] args)
        {
            string totalSearchPattern = "AllPackagesIssues.json*";
            string packageASearchPattern = "TotalIssuesA*.json*";
            string directoryPath = "../../../../ReportsTest";
            string summaryTotalJson = ReadFileWithFuzzyMatch(directoryPath, totalSearchPattern);
            string packageATotalJson = ReadFileWithFuzzyMatch(directoryPath, packageASearchPattern);
            string updatedTotalJsonPath = $"{directoryPath}/final.json";

            if (!string.IsNullOrEmpty(summaryTotalJson) && !string.IsNullOrEmpty(packageATotalJson))
            {
                JArray totalArray = JArray.Parse(summaryTotalJson);
                JArray packageArray = JArray.Parse(packageATotalJson);

                bool packageAExists = false;
                JObject packageAObj = null;

                foreach (JObject totalItem in totalArray)
                {
                    if (totalItem["PackageName"]?.ToString() == "packageA")
                    {
                        packageAExists = true;
                        packageAObj = totalItem;
                        break;
                    }
                }
                
                if (packageAExists)
                {
                    packageAObj["ResultList"] = packageArray;
                }
                else
                {
                    JObject newPackageA = new JObject
                    {
                        { "PackageName", "packageA" },
                        { "ResultList", packageArray },
                        { "Note", null }
                    };
                    totalArray.Add(newPackageA);
                }

                string updatedTotalJson = totalArray.ToString(Formatting.Indented);
                File.WriteAllText(updatedTotalJsonPath, updatedTotalJson);
            }
            else if(string.IsNullOrEmpty(summaryTotalJson))
            {
                Console.WriteLine("No file found matching the summary total issue.");
            }
            else
            {
                Console.WriteLine("No file found matching the single package total issue.");
            }
        }

        static string ReadFileWithFuzzyMatch(string directory, string searchPattern){
            string[] matchingFiles = Directory.GetFiles(directory, searchPattern, SearchOption.AllDirectories);

            if (matchingFiles.Length == 0)
            {
                Console.WriteLine("The current work path is: " + Directory.GetCurrentDirectory());
                return string.Empty;
            }

            return File.ReadAllText(matchingFiles[0]);
        }
    }
}