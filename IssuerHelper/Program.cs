using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IssuerHelper
{
    public class Issuer
    {
        static void Main(string[] args)
        {
            // Default Configuration
            using IHost host = Host.CreateApplicationBuilder(args).Build();

            IConfiguration config = host.Services.GetRequiredService<IConfiguration>();

            string? packages = config["PackageName"];

            Console.WriteLine("Running packages: " + packages);

            string[]? allPackages = ParseInputPackages(packages);

            string totalSearchPattern = "SummaryTotalIssues.json";
            string totalIssueSummaryPath = "../Artifacts";
            
            string reportPath = "../eng";
            string updatedTotalJsonPath = $"{reportPath}/{totalSearchPattern}";
            string summaryTotalJson = ReadFileWithFuzzyMatch(totalIssueSummaryPath, totalSearchPattern);

            foreach(var package in allPackages){
                string packageFilePath = $"../Artifacts/{package}";
                string packageASearchPattern = "TotalIssues*.json";
                string packageATotalJson = ReadFileWithFuzzyMatch(packageFilePath, packageASearchPattern);
                
                if (!string.IsNullOrEmpty(summaryTotalJson) && !string.IsNullOrEmpty(packageATotalJson))
                {
                    Console.WriteLine("111111111111111111111111111111111111");
                    JArray totalArray = JArray.Parse(summaryTotalJson);
                    JArray packageArray = JArray.Parse(packageATotalJson);

                    bool packageAExists = false;
                    JObject packageAObj = null;

                    foreach (JObject totalItem in totalArray)
                    {
                        if (totalItem["PackageName"]?.ToString() == package)
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
                            { "PackageName", package },
                            { "ResultList", packageArray },
                            { "Note", null }
                        };
                        totalArray.Add(newPackageA);
                    }

                    summaryTotalJson = totalArray.ToString(Formatting.Indented);
                }
                else if(string.IsNullOrEmpty(summaryTotalJson) && !string.IsNullOrEmpty(packageATotalJson))
                {
                    Console.WriteLine("22222222222222222222222222222222");
                    string totalJsonContent = "[]";
                    JArray? totalArray = JsonConvert.DeserializeObject<JArray>(totalJsonContent);
                    JArray packageArray = JArray.Parse(packageATotalJson);
                    JObject newPackageA = new JObject
                    {
                        { "PackageName", package },
                        { "ResultList", packageArray },
                        { "Note", null }
                    };
                    totalArray?.Add(newPackageA);
                    summaryTotalJson = totalArray?.ToString(Formatting.Indented);
                }
                else
                {
                    Console.WriteLine($"No total issue file found matching the single package: {package}.");
                }
            }
            File.WriteAllText(updatedTotalJsonPath, summaryTotalJson);
        }

        static string ReadFileWithFuzzyMatch(string directory, string searchPattern){
            if(Directory.Exists(directory)){
                string[] matchingFiles = Directory.GetFiles(directory, searchPattern, SearchOption.AllDirectories);

                if (matchingFiles.Length == 0)
                {
                    return string.Empty;
                }
                return File.ReadAllText(matchingFiles[0]);
            }
            else{
                return string.Empty;
            }
        }

        static string[]? ParseInputPackages(string? packages){
            if(packages.Equals("all")){
                string packagesFilePath = "ConfigureAllPackages.json";
                string content = File.ReadAllText(packagesFilePath);
                JArray jsonArray = JArray.Parse(content);
                JObject jsonObject = (JObject)jsonArray[0];

                JArray packagesArray = (JArray)jsonObject["packages"];
                string[] parsedPackages = new string[packagesArray.Count];
                for (int i = 0; i < packagesArray.Count; i++)
                {
                    parsedPackages[i] = (string)packagesArray[i];
                }
                return parsedPackages;
            }
            else{
                return packages?.Replace(" ","").Split(",");
            }
        }
    }
}