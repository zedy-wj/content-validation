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


            var allPackages = await data.FetchPackages(dataUrl);
            System.Console.WriteLine("Pending Testing Packages for This Month");
        }

        public async Task<string> FetchPackages(string testLink)
        {
            var browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var page = await browser.NewPageAsync();
            await PlaywrightHelper.GotoageWithRetriesAsync(page, testLink);

            // Fetch content of textarea#read-only-cursor-text-area in {language}.yml file.
            var textAreaContent = await page.EvaluateAsync<string>("() => document.querySelector('textarea#read-only-cursor-text-area')?.value");

            var lines = textAreaContent.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var result = new List<Dictionary<string, string>>();

            string? currentName = null;
            string? currentVersionType = null;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith("- Name:"))
                {
                    currentName = trimmedLine.Substring("- Name:".Length).Trim();
                }
                else if (trimmedLine.StartsWith("VersionType:"))
                {
                    currentVersionType = trimmedLine.Substring("VersionType:".Length).Trim();
                }

                if (currentName != null && currentVersionType != null)
                {
                    var entry = new Dictionary<string, string>
                    {
                        { "Name", currentName },
                        { "VersionType", currentVersionType }
                    };

                    // Check if the entry already exists in the result list
                    if (!result.Any(e => e["Name"] == currentName && e["VersionType"] == currentVersionType))
                    {
                        result.Add(entry);
                    }

                    // Reset fields to parse the next entry
                    currentName = null;
                    currentVersionType = null;
                }
            }

            result = PythonFilterPackages(result);

            // Convert to JSON format
            var jsonResult = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });

            // 将 JSON 写入到 result.json 文件
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "result.json");
            await File.WriteAllTextAsync(filePath, jsonResult);
            System.Console.WriteLine($"JSON has been written to {filePath}");
            return jsonResult == null ? throw new InvalidOperationException("Failed to serialize packages to JSON.") : jsonResult;
        }

        public List<Dictionary<string, string>>PythonFilterPackages(List<Dictionary<string, string>> result)
        {

            // 移除以 "azure-mgmt-" 开头的条目
            result.RemoveAll(package => package.ContainsKey("Name") && package["Name"].StartsWith("azure-mgmt-"));

            return result;
        }
    }
}