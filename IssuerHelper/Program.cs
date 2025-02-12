﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http.Headers;
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

            string packages = config["PackageName"] ?? "all";

            Console.WriteLine("Running packages: " + packages);

            // Parse all packages in this pipeline run
            string[] allPackages = ParseInputPackages(packages);

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
            GenerateAllPackageExcelFile(updatedSummaryJsonPath);

            UploadCurrentPipelineTotalIssuesArtifact(allPackages, packageATotalSearchPattern, updatedTotalJsonPath);
            GenerateAllPackageExcelFile(updatedTotalJsonPath);

            UploadCurrentPipelineDiffIssuesArtifact(allPackages, packageADiffSearchPattern, updatedDiffJsonPath);
            GenerateAllPackageExcelFile(updatedDiffJsonPath);
            
            GenerateMarkDownFile(allPackages);

        }

        static string ReadFileWithFuzzyMatch(string directory, string searchPattern)
        {
            if (Directory.Exists(directory))
            {
                string[] matchingFiles = Directory.GetFiles(directory, searchPattern, SearchOption.AllDirectories);

                if (matchingFiles.Length == 0)
                {
                    return string.Empty;
                }
                return File.ReadAllText(matchingFiles[0]);
            }
            else
            {
                return "Failed";
            }
        }

        static string[] ParseInputPackages(string packages)
        {
            if (packages.Equals("all"))
            {
                string packagesFilePath = "ConfigureAllPackages.json";
                string content = File.ReadAllText(packagesFilePath);
                JArray jsonArray = JArray.Parse(content);
                JObject jsonObject = (JObject)jsonArray[0];

                JArray packagesArray = (JArray)jsonObject["packages"]!;
                string[] parsedPackages = new string[packagesArray.Count];
                for (int i = 0; i < packagesArray.Count; i++)
                {
                    parsedPackages[i] = (string)packagesArray[i]!;
                }
                return parsedPackages;
            }
            else
            {
                return packages.Replace(" ", "").Split(",");
            }
        }

        static void CreateArtifactsFolder(string folder)
        {
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

        static void UploadSummaryIssuesArtifact(string[] allPackages, string summaryTotalJson, string updatedSummaryJsonPath, string packageASearchPattern)
        {

            foreach (var package in allPackages)
            {
                string packageFilePath = $"../Artifacts/{package}";

                string packageATotalJson = ReadFileWithFuzzyMatch(packageFilePath, packageASearchPattern);

                if (packageATotalJson.Equals("Failed"))
                {
                    Console.WriteLine($"The package {package} failed in pipeline run, please check it.");
                    continue;
                }
                if (!string.IsNullOrEmpty(summaryTotalJson) && !string.IsNullOrEmpty(packageATotalJson))
                {
                    JArray totalArray = JArray.Parse(summaryTotalJson);
                    JArray packageArray = JArray.Parse(packageATotalJson);

                    bool packageAExists = false;
                    JObject? packageAObj = null;

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
                        packageAObj!["ResultList"] = packageArray;
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
                else if (string.IsNullOrEmpty(summaryTotalJson) && !string.IsNullOrEmpty(packageATotalJson))
                {
                    string totalJsonContent = "[]";
                    JArray totalArray = JsonConvert.DeserializeObject<JArray>(totalJsonContent)!;
                    JArray packageArray = JArray.Parse(packageATotalJson);
                    JObject newPackageA = new JObject
                    {
                        { "PackageName", package },
                        { "ResultList", packageArray },
                        { "Note", null }
                    };
                    totalArray.Add(newPackageA);
                    summaryTotalJson = totalArray.ToString(Formatting.Indented);
                }
                else
                {
                    Console.WriteLine($"No total issue file found matching the single package: {package}.");
                }
            }
            File.WriteAllText(updatedSummaryJsonPath, summaryTotalJson);
        }

        static void UploadCurrentPipelineTotalIssuesArtifact(string[] allPackages, string packageATotalSearchPattern, string updatedTotalJsonPath)
        {
            string totalJsonContent = "[]";
            JArray totalArray = JsonConvert.DeserializeObject<JArray>(totalJsonContent)!;

            foreach (var package in allPackages)
            {
                string packageFilePath = $"../Artifacts/{package}";

                string packageATotalJson = ReadFileWithFuzzyMatch(packageFilePath, packageATotalSearchPattern);

                if (packageATotalJson.Equals("Failed"))
                {
                    Console.WriteLine($"The package {package} failed in pipeline run, please check it.");
                    continue;
                }
                if (!string.IsNullOrEmpty(packageATotalJson))
                {
                    JArray packageArray = JArray.Parse(packageATotalJson);
                    JObject newPackageA = new JObject
                    {
                        { "PackageName", package },
                        { "ResultList", packageArray },
                        { "Note", null }
                    };
                    totalArray.Add(newPackageA);
                    totalJsonContent = totalArray.ToString(Formatting.Indented);
                }
                else
                {
                    Console.WriteLine($"Package: {package} in this pipeline is pass.");
                }
            }
            File.WriteAllText(updatedTotalJsonPath, totalJsonContent);
        }

        static void UploadCurrentPipelineDiffIssuesArtifact(string[] allPackages, string packageADiffSearchPattern, string updatedDiffJsonPath)
        {
            string diffJsonContent = "[]";
            JArray totalArray = JsonConvert.DeserializeObject<JArray>(diffJsonContent)!;

            foreach (var package in allPackages)
            {
                string packageFilePath = $"../Artifacts/{package}";

                string packageADiffJson = ReadFileWithFuzzyMatch(packageFilePath, packageADiffSearchPattern);

                if (packageADiffJson.Equals("Failed"))
                {
                    Console.WriteLine($"The package {package} failed in pipeline run, please check it.");
                    continue;
                }
                if (!string.IsNullOrEmpty(packageADiffJson))
                {
                    JArray packageArray = JArray.Parse(packageADiffJson);
                    JObject newPackageA = new JObject
                    {
                        { "PackageName", package },
                        { "ResultList", packageArray },
                        { "Note", null }
                    };
                    totalArray.Add(newPackageA);
                    diffJsonContent = totalArray.ToString(Formatting.Indented);
                }
                else
                {
                    Console.WriteLine($"Package: {package} in this pipeline has no diff issues or failed in pipeline.");
                }
            }
            File.WriteAllText(updatedDiffJsonPath, diffJsonContent);
        }

        static void GenerateMarkDownFile(string[] packages)
        {
            string markdownTable = $@"
| id | package | status | issue link | created date of issue | update date of issue | run date of pipeline |
|----|---------|--------|------------|-----------------------|----------------------| ---------------------|";

            int index = 1;
            DateTime now = DateTime.Now;
            foreach (var package in packages)
            {
                string packageFilePath = $"../Artifacts/{package}";
                string IssueSearchPattern = "TotalIssues*.json";
                string packageIssueInfo = ReadFileWithFuzzyMatch(packageFilePath, IssueSearchPattern);
                var issueObject = GetIssueInfo(package);

                if (packageIssueInfo.Equals("Failed"))
                {
                    Console.WriteLine($"The package {package} failed in pipeline run, please check it.");
                    markdownTable += $@"
| {index} | {package} | Pipeline fail | / | / | / | {now.ToString("M/d/yyyy h:mm:ss tt")} |";
                    index++;
                    continue;
                }
                if (string.IsNullOrEmpty(packageIssueInfo) && issueObject == null)
                {
                    markdownTable += $@"
| {index} | {package} | PASS | / | / | / | {now.ToString("M/d/yyyy h:mm:ss tt")} |";
                }
                else if (string.IsNullOrEmpty(packageIssueInfo) && issueObject != null)
                {
                    markdownTable += $@"
| {index} | {package} | PASS | {issueObject["html_url"]?.ToString()} | {issueObject["created_at"]?.ToObject<DateTime>()} | {issueObject["updated_at"]?.ToObject<DateTime>()} | {now.ToString("M/d/yyyy h:mm:ss tt")} |";
                }
                else
                {
                    markdownTable += $@"
| {index} | {package} | Test fail | {issueObject?["html_url"]?.ToString()} | {issueObject?["created_at"]?.ToObject<DateTime>()} | {issueObject?["updated_at"]?.ToObject<DateTime>()} | {now.ToString("M/d/yyyy h:mm:ss tt")} |";
                }
                index++;
            }

            string filePath = $"../latest-pipeline-result.md";

            try
            {
                File.WriteAllText(filePath, markdownTable);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Occurrence error when creating md file: {ex.Message}");
            }
        }

        static JToken? GetIssueInfo(string package)
        {
            string owner = "zedy-wj";
            string repo = "content-validation";
            string issueTitle = $"{package} content validation issue for learn microsoft website.";
            string apiUrl = $"https://api.github.com/repos/{owner}/{repo}/issues";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("MyGitHubApp", "1.0"));
                    HttpResponseMessage response = client.GetAsync(apiUrl).Result;
                    response.EnsureSuccessStatusCode();

                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    JArray issues = JArray.Parse(responseBody);

                    var matchingIssue = issues.FirstOrDefault(i => (string?)i["title"] == issueTitle);

                    if (matchingIssue != null)
                    {
                        Console.WriteLine($"Html url: {matchingIssue["html_url"]}");
                        Console.WriteLine($"Created at: {matchingIssue["created_at"]}");
                        Console.WriteLine($"Updated at: {matchingIssue["updated_at"]}");
                        return matchingIssue;
                    }
                    else
                    {
                        Console.WriteLine($"No issue found with title: {issueTitle}");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred:");
                Console.WriteLine(ex.Message);
                return null;
            }
        }


        static void GenerateAllPackageExcelFile(string inputJsonPath)
        {
            List<TPackage4Json> allPackageList = JsonConvert.DeserializeObject<List<TPackage4Json>>(File.ReadAllText(inputJsonPath)) ?? new List<TPackage4Json>();
            string outputExcelPath =  Path.ChangeExtension(inputJsonPath, ".xlsx");

            for (int i = 0; i < allPackageList.Count; i++)
            {
                if (allPackageList[i].ResultList != null || allPackageList[i].ResultList != null)
                {
                    ReportHelper.ExcelHelper4Test.AddTestResult(allPackageList[i].ResultList!, outputExcelPath, allPackageList[i].PackageName!);
                }
            }
        }

    }
}