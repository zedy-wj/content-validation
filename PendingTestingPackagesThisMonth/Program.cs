using Microsoft.Playwright;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace PendingTestingPackagesThisMonth
{
    public class PendingTestingPackagesThisMonth
    {
        private static readonly string RELEASE_PACKAGES_URL_PREFIX = "https://github.com/Azure/azure-sdk/tree/main/_data/releases/";
        private IPlaywright _playwright;
        private string BackupFilePath = Path.Combine(Directory.GetCurrentDirectory(), "run-all-packages.yml.backup");

        public PendingTestingPackagesThisMonth(IPlaywright playwright)
        {
            _playwright = playwright ?? throw new ArgumentNullException(nameof(playwright));
        }
        static async Task Main(string[] args)
        {
            // Default Configuration
            using IHost host = Host.CreateApplicationBuilder(args).Build();
            IConfiguration config = host.Services.GetRequiredService<IConfiguration>();
            string? language = config["Language"];

            // Initialize Playwright instance.
            if (string.IsNullOrEmpty(language))
            {
                throw new ArgumentException("Language must be specified in the configuration.");
            }
            else if (language.ToLower() == "javascript")
            {
                language = "js";
            }
            IPlaywright playwright = Playwright.CreateAsync().GetAwaiter().GetResult();
            PendingTestingPackagesThisMonth data = new PendingTestingPackagesThisMonth(playwright);

            /*
            ** Retrieve the current system date and pick the corresponding folder from the GitHub repo.
            ** The folder name is expected to be in the format "YYYY-MM".
            ** For example, if the current date is August 11, 2025, the folder name would be "2025-07".
            ** Notes: Test the packages released last month, as the current month's may not have all been released yet.
            */
            var currentDate = DateTime.Now;
            var folderName = currentDate.Month == 1
                ? $"{currentDate.Year - 1}-12"
                : $"{currentDate.Year}-{currentDate.Month - 1:D2}";
            var dataUrl = $"{RELEASE_PACKAGES_URL_PREFIX}{folderName}/{language.ToLower()}.yml";


            var packages = await data.FetchPackages(dataUrl, language);
            System.Console.WriteLine("Pending Testing Packages for This Month");
        }

        public async Task<string> FetchPackages(string testLink, string language)
        {
            var browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var page = await browser.NewPageAsync();
            await PlaywrightHelper.GotoageWithRetriesAsync(page, testLink);

            // Fetch content of textarea#read-only-cursor-text-area in {language}.yml file.
            var textAreaContent = await page.EvaluateAsync<string>("() => document.querySelector('textarea#read-only-cursor-text-area')?.value");

            var lines = textAreaContent.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var result = new HashSet<string>();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith("- Name:"))
                {
                    var packageName = trimmedLine.Substring("- Name:".Length).Trim();
                    if (!string.IsNullOrEmpty(packageName))
                    {
                        result.Add(packageName.Replace("'", ""));
                    }
                }
            }

            HashSet<string> filteredResult = language.ToLower() switch
            {
                "python" => await PythonFilterPackages(result),
                "java" => await JavaFilterPackages(result),
                "dotnet" => await DotNetFilterPackages(result),
                "js" => await JavaScriptFilterPackages(result),
                _ => result
            };

            // Convert to JSON format
            var jsonResult = JsonSerializer.Serialize(filteredResult.ToList(), new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            return jsonResult == null ? throw new InvalidOperationException("Failed to serialize packages to JSON.") : jsonResult;
        }

        public async Task<HashSet<string>> PythonFilterPackages(HashSet<string> result)
        {
            result.RemoveWhere(packageName => packageName.StartsWith("azure-mgmt-"));

            var outputFilePath = Path.Combine(Directory.GetCurrentDirectory(), "../eng/pipelines/python/run-all-packages.yml");

            // Create packages string in YAML list format
            var packagesList = string.Join("\n    - ", result.OrderBy(p => p));
            var packagesYaml = $"- {packagesList}";

            await GenerateYmlFile(outputFilePath, packagesYaml, "python");

            return result;
        }

        public async Task<HashSet<string>> JavaFilterPackages(HashSet<string> result)
        {
            result.RemoveWhere(packageName => packageName.StartsWith("azure-resourcemanager-"));

            var outputFilePath = Path.Combine(Directory.GetCurrentDirectory(), "../eng/pipelines/java/run-all-packages.yml");

            // Create packages string in YAML list format
            var packagesList = string.Join("\n    - ", result.OrderBy(p => p));
            var packagesYaml = $"- {packagesList}";

            await GenerateYmlFile(outputFilePath, packagesYaml, "java");

            return result;
        }

        public async Task<HashSet<string>> DotNetFilterPackages(HashSet<string> result)
        {
            result.RemoveWhere(packageName => packageName.StartsWith("Azure.ResourceManager."));

            // Update package names to lowercase and replace "." with "-"
            var updatedPackages = result.Select(p => p.Replace(".", "-").ToLower()).ToList();

            var outputFilePath = Path.Combine(Directory.GetCurrentDirectory(), "../eng/pipelines/dotnet/run-all-packages.yml");

            // Create packages string in YAML list format
            var packagesList = string.Join("\n    - ", updatedPackages.OrderBy(p => p));
            var packagesYaml = $"- {packagesList}";

            await GenerateYmlFile(outputFilePath, packagesYaml, "dotnet");

            return result;
        }

        public async Task<HashSet<string>> JavaScriptFilterPackages(HashSet<string> result)
        {
            result.RemoveWhere(packageName => packageName.StartsWith("@azure/arm-") || packageName.StartsWith("@azure-rest/"));

            // Update package names to lowercase and replace "." with "-"
            var updatedPackages = result.Select(p => p.Replace("@", "").Replace("/", "-").ToLower()).ToList();

            var outputFilePath = Path.Combine(Directory.GetCurrentDirectory(), "../eng/pipelines/javascript/run-all-packages.yml");

            // Create packages string in YAML list format
            var packagesList = string.Join("\n    - ", updatedPackages.OrderBy(p => p));
            var packagesYaml = $"- {packagesList}";

            await GenerateYmlFile(outputFilePath, packagesYaml, "javascript");

            return result;
        }

        private async Task GenerateYmlFile(string outputFilePath, string packagesYaml, string language)
        {
            if (!File.Exists(BackupFilePath))
            {
                System.Console.WriteLine($"Backup file not found: {BackupFilePath}");
                return;
            }

            // Read backup file content
            var templateContent = await File.ReadAllTextAsync(BackupFilePath);

            // Replace placeholders
            var finalContent = templateContent
                .Replace("${Packages}", packagesYaml)
                .Replace("${language}", language);

            // Write to output file
            await File.WriteAllTextAsync(outputFilePath, finalContent);
            System.Console.WriteLine($"YAML file has been written to {outputFilePath}");
        }
    }
}