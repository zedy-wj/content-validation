using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using Microsoft.Playwright;

namespace DataSource
{
    internal class Program
    {
        private static readonly string Java_SDK_API_URL_PREFIX = "https://learn.microsoft.com/en-us/java/api/overview/azure/";
        private static readonly string Java_SDK_API_URL_BASIC = "https://learn.microsoft.com/en-us/java/api/";
        public static async Task Main(string[] args)
        {
            // Default Configuration
            using IHost host = Host.CreateApplicationBuilder(args).Build();

            IConfiguration config = host.Services.GetRequiredService<IConfiguration>();

            string? packagename = config["PackageName"];

            List<string> allpages = new List<string>();

            await Run(packagename, allpages);

            ExportData(allpages);

            host.RunAsync();
        }

        public async static Task Run(string package, List<string> allpages)
        {
            List<string> pages = new List<string>();

            string pagelink = Java_SDK_API_URL_PREFIX + package;

            await GetAllChildPage(pages, allpages, pagelink);
        }


        // Get all child pages of the API reference document by using `palywright`
        static async Task GetAllChildPage(List<string> pages, List<string> allpages, string pagelink)
        {
            // Launch a browser
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
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

                await browser.CloseAsync();

                // Recursively get all pages of the API reference document
                foreach (var pa in pages)
                {
                    allpages.Add(pa);
                    GetAllPages(pa, allpages);
                }
            }
        }

        public static bool IsTrue(string link)
        {
            var web = new HtmlWeb();
            var doc = web.Load(link);
            string? hNode;

            hNode = doc.DocumentNode.SelectSingleNode("//h1")?.InnerText;
            if (!string.IsNullOrEmpty(hNode) && hNode.Contains("Package"))
            {
                return true;
            }

            // Determine if the page contains the specified content "Classes"
            hNode = doc.DocumentNode.SelectSingleNode("//h2[@id='classes']")?.InnerText;
            if (!string.IsNullOrEmpty(hNode) && hNode.Contains("Classes"))
            {
                return true;
            }

            // Determine if the page contains the specified content "Interfaces"
            hNode = doc.DocumentNode.SelectSingleNode("//h2[@id='interfaces']")?.InnerText;
            if (!string.IsNullOrEmpty(hNode) && hNode.Contains("Interfaces"))
            {
                return true;
            }

            // Determine if the page contains the specified content "Enums"
            hNode = doc.DocumentNode.SelectSingleNode("//h2[@id='enums']")?.InnerText;
            if (!string.IsNullOrEmpty(hNode) && hNode.Contains("Enums"))
            {
                return true;
            }

            return false;
        }

        public static void GetAllPages(string apiRefDocPage, List<string> links)
        {
            HtmlWeb web = new HtmlWeb();
            var doc = web.Load(apiRefDocPage);

            //The recursion terminates when there are no valid sub pages in the page or when all package links have been visited.
            if (IsTrue(apiRefDocPage))
            {
                // Get all a tags in method table
                var aNodes = doc.DocumentNode.SelectNodes("//td/span/a");

                if (aNodes != null)
                {
                    foreach (var node in aNodes)
                    {
                        string href = Java_SDK_API_URL_BASIC + node.Attributes["href"]?.Value;

                        // Determine if the page has been contained
                        if (!links.Contains(href))
                        {
                            links.Add(href);
                            GetAllPages(href, links);
                        }
                    }
                }
            }
        }

        public static void ExportData(List<string> pages)
        {
            string jsonString = JsonSerializer.Serialize(pages);
            File.WriteAllText("../ContentValidation.Test/appsettings.json", jsonString);
        }
    }
}
