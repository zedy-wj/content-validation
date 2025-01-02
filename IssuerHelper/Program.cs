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

            // Parse all packages in this pipeline run
            string[]? allPackages = ParseInputPackages(packages);

            // Create the upload folder
            string reportPath = "../Reports";
            CreateArtifactsFolder(reportPath);

            string summarySearchPattern = "HistorySummaryTotalIssues.json";
            string packageATotalSearchPattern = "TotalIssues*.json";
            string packageADiffSearchPattern = "DiffIssues*.json";
            string totalIssueSummaryPath = "../Artifacts";
            
            string updatedSummaryJsonPath = $"{reportPath}/{summarySearchPattern}";
            string updatedTotalJsonPath = $"{reportPath}/CurrentPipelineTotalIssues.json";
            string updatedDiffJsonPath = $"{reportPath}/CurrentPipelineDiffIssues.json";

            string summaryTotalJson = ReadFileWithFuzzyMatch(totalIssueSummaryPath, summarySearchPattern);

            UploadSummaryIssuesArtifact(allPackages, summaryTotalJson, updatedSummaryJsonPath, packageATotalSearchPattern);

            UploadCurrentPipelineTotalIssuesArtifact(allPackages, packageATotalSearchPattern, updatedTotalJsonPath);

            UploadCurrentPipelineDiffIssuesArtifact(allPackages, packageADiffSearchPattern, updatedDiffJsonPath);

            GenerateMarkDownFile(allPackages);
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

        static void CreateArtifactsFolder(string folder){
            string folderPath = folder;

            try
            {
                // Check the folder exist
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
            }
            catch (Exception ex)
            {
                // Catch the exception
                Console.WriteLine("Occurrence error when creating folder Reports: " + ex.Message);
            }
        }

        static void UploadSummaryIssuesArtifact(string[]? allPackages, string summaryTotalJson, string updatedSummaryJsonPath, string packageASearchPattern){

            foreach(var package in allPackages){
                string packageFilePath = $"../Artifacts/{package}";
                
                string packageATotalJson = ReadFileWithFuzzyMatch(packageFilePath, packageASearchPattern);
                
                if (!string.IsNullOrEmpty(summaryTotalJson) && !string.IsNullOrEmpty(packageATotalJson))
                {
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
            File.WriteAllText(updatedSummaryJsonPath, summaryTotalJson);
        }

        static void UploadCurrentPipelineTotalIssuesArtifact(string[]? allPackages, string packageATotalSearchPattern, string updatedTotalJsonPath){
            string totalJsonContent = "[]";
            JArray? totalArray = JsonConvert.DeserializeObject<JArray>(totalJsonContent);

            foreach(var package in allPackages){
                string packageFilePath = $"../Artifacts/{package}";
                
                string packageATotalJson = ReadFileWithFuzzyMatch(packageFilePath, packageATotalSearchPattern);

                if(!string.IsNullOrEmpty(packageATotalJson))
                {
                    JArray packageArray = JArray.Parse(packageATotalJson);
                    JObject newPackageA = new JObject
                    {
                        { "PackageName", package },
                        { "ResultList", packageArray },
                        { "Note", null }
                    };
                    totalArray?.Add(newPackageA);
                    totalJsonContent = totalArray?.ToString(Formatting.Indented);
                }
                else
                {
                    Console.WriteLine($"Package: {package} in this pipeline is pass.");
                }
            }
            File.WriteAllText(updatedTotalJsonPath, totalJsonContent);
        }

        static void UploadCurrentPipelineDiffIssuesArtifact(string[]? allPackages, string packageADiffSearchPattern, string updatedDiffJsonPath){
            string diffJsonContent = "[]";
            JArray? totalArray = JsonConvert.DeserializeObject<JArray>(diffJsonContent);

            foreach(var package in allPackages){
                string packageFilePath = $"../Artifacts/{package}";
                
                string packageADiffJson = ReadFileWithFuzzyMatch(packageFilePath, packageADiffSearchPattern);

                if(!string.IsNullOrEmpty(packageADiffJson))
                {
                    JArray packageArray = JArray.Parse(packageADiffJson);
                    JObject newPackageA = new JObject
                    {
                        { "PackageName", package },
                        { "ResultList", packageArray },
                        { "Note", null }
                    };
                    totalArray?.Add(newPackageA);
                    diffJsonContent = totalArray?.ToString(Formatting.Indented);
                }
                else
                {
                    Console.WriteLine($"Package: {package} in this pipeline has no diff issues.");
                }
            }
            File.WriteAllText(updatedDiffJsonPath, diffJsonContent);
        }

        static void GenerateMarkDownFile(string[] packages){
            string markdownTable = $@"
| id | package | status | issue link | created date of issue | update date of issue |
|----|---------|--------|------------|-----------------------|----------------------|";
            
            int index = 1;
            foreach(var package in packages){
                string packageFilePath = $"../Artifacts/{package}";
                string IssueSearchPattern = "IssueStatusInfo.json";
                string packageIssueInfo = ReadFileWithFuzzyMatch(packageFilePath, IssueSearchPattern);
                if(string.IsNullOrEmpty(packageIssueInfo)){
                    markdownTable += $@"
| {index} | {package} | PASS | / | / | / |";
                }
                else
                {
                    try
                    {
                        JObject issueObject = JObject.Parse(packageIssueInfo);
            
                        markdownTable += $@"
| {index} | {package} | {issueObject["status"]?.ToString()} | {issueObject["url"]?.ToString()} | {issueObject["created_at"]?.ToObject<DateTime>()} | {issueObject["updated_at"]?.ToObject<DateTime>()} |";
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error parsing JSON: " + ex.Message);
                    }
                }
                index++;
            }

            DateTime now = DateTime.Now;
            
            string filePath = $"../pipeline-result-{now.ToString("yyyy-MM-dd")}.md";
    
            try
            {
                File.WriteAllText(filePath, markdownTable);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Occurrence error when creating md file: {ex.Message}");
            }
        }
    }
}