using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using Microsoft.Playwright;

namespace DataSource
{
    public class GetDataSource
    {
        private static readonly string SDK_API_URL_BASIC = "https://learn.microsoft.com/en-us/";
        static async Task Main(string[] args)
        {
            // Default Configuration
            // using IHost host = Host.CreateApplicationBuilder(args).Build();

            // IConfiguration config = host.Services.GetRequiredService<IConfiguration>();

            string? package = "search-documents-readme";
            string? language = "Javascript";
            // string? package = config["PackageName"];
            // string? language = config["Language"];

            string? overview_url = GetLanguagePageOverview(language);

            List<string> pages = new List<string>();
            List<string> allpages = new List<string>();
            string pagelink = $"{overview_url}/{package}";

            await GetAllChildPage(pages, allpages, pagelink);

            foreach (var page in allpages)
            {
                Console.WriteLine(page);
            }
            Console.WriteLine(allpages.Count);
            // ExportData(allpages);

            // host.RunAsync();
        }

        static string GetLanguagePageOverview(string? language)
        {
            language = language?.ToLower();
            return $"{SDK_API_URL_BASIC}/{language}/api/overview/azure/";
        }

        static async Task GetAllChildPage(List<string> pages, List<string> allpages, string pagelink)
        {
            // Launch a browser
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false
            });

            var page = await browser.NewPageAsync();
            IReadOnlyList<ILocator> links = new List<ILocator>();

            // Retry 5 times to get the child pages if cannot get pagelinks.
            int i = 0;
            while (links.Count == 0)
            {
                // If the page does not contain the specified content, break the loop
                if (i == 5)
                {
                    break;
                }

                await page.GotoAsync(pagelink);

                // Get all child pages
                links = await page.Locator("li.tree-item.is-expanded ul.tree-group a").AllAsync();
                
                i++;
            }

            if (links.Count != 0)
            {
                // Get all href attributes of the child pages
                foreach (var link in links)
                {
                    var href = await link.GetAttributeAsync("href");
                    pages.Add(href);
                }

                // await browser.CloseAsync();

                // Recursively get all pages of the API reference document
                foreach (var pa in pages)
                {
                    int lastSlashIndex = pa.LastIndexOf('/');
                    string baseUri = pa.Substring(0, lastSlashIndex + 1);
                    allpages.Add(pa);
                    GetAllPages(pa, baseUri, allpages);
                }
            }
        }

        static void GetAllPages(string apiRefDocPage, string? baseUri, List<string> links)
        {
            HtmlWeb web = new HtmlWeb();
            var doc = web.Load(apiRefDocPage);

            //The recursion terminates when there are no valid sub pages in the page or when all package links have been visited.
            if (IsTrue(apiRefDocPage))
            {
                // Console.WriteLine("今天天气不错，适合散步");
                var aNodes = doc.DocumentNode.SelectNodes("//td/a | //td/span/a");

                if (aNodes != null)
                {
                    // Console.WriteLine("今天天气不错，适合散步");
                    foreach (var node in aNodes)
                    {
                        string href = $"{baseUri}/" + node.Attributes["href"].Value;

                        if (!links.Contains(href))
                        {
                            links.Add(href);

                            //Call GetAllLinks method recursively for each new link.
                            GetAllPages(href, baseUri, links);
                        }
                    }
                }
            }
        }

        static bool IsTrue(string link)
        {
            var web = new HtmlWeb();
            var doc = web.Load(link);
            var checks = new[]
            {
                new { XPath = "//h1", Content = "Package" },
                new { XPath = "//h2[@id='classes']", Content = "Classes" },
                new { XPath = "//h2[@id='interfaces']", Content = "Interfaces" },
                new { XPath = "//h2[@id='structs']", Content = "Structs" },
                new { XPath = "//h2[@id='typeAliases']", Content = "Type Aliases" },
                new { XPath = "//h2[@id='functions']", Content = "Functions" },
                new { XPath = "//h2[@id='enums']", Content = "Enums" }
            };

            foreach (var check in checks)
            {
                string? hNode = doc.DocumentNode.SelectSingleNode(check.XPath)?.InnerText;
                if (!string.IsNullOrEmpty(hNode) && hNode.Contains(check.Content))
                {
                    return true;
                }
            }

            return false;
        }
        
        static void ExportData(List<string> pages)
        {
            string jsonString = JsonSerializer.Serialize(pages);
            Console.WriteLine(jsonString);
            File.WriteAllText("../ContentValidation.Test/appsettings.json", jsonString);
        }
    }
}