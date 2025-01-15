using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using Microsoft.Playwright;
using System;

namespace GetAllLink
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

            List<string> pages = new List<string>();
            List<string> allpages = new List<string>();

            string pagelink = Java_SDK_API_URL_PREFIX + packagename;

            await GetAllChildPage(pages, allpages, pagelink);

            ExportData(allpages);

            host.RunAsync();
        }

        /// <summary>
        /// Get all child pages of the API reference document by using `palywright`
        /// </summary>
        /// <param name="pages"></param>
        /// <param name="allpages"></param>
        /// <param name="pagelink"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Recursively get all pages of the API reference document
        /// </summary>
        /// <param name="apiRefDocPage"></param>
        /// <param name="links"></param>
        static void GetAllPages(string apiRefDocPage, List<string> links)
        {
            // Load the page
            var web = new HtmlWeb();
            var doc = web.Load(apiRefDocPage);

            // Determine if recursion is required
            if (isTrue(apiRefDocPage))
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

        /// <summary>
        /// Determine if recursion is required
        /// </summary>
        /// <param name="link">The url to be judged</param>
        /// <returns>True or False</returns>
        static bool isTrue(string link)
        {
            var web = new HtmlWeb();
            var doc = web.Load(link);

            // Determine if the page contains the specified content "Classes"
            var h2Node = doc.DocumentNode.SelectSingleNode("//h2[@id='classes']")?.InnerText;
            if (h2Node != null && h2Node.Contains("Classes"))
            {
                return true;
            }

            // Determine if the page contains the specified content "Interfaces"
            h2Node = doc.DocumentNode.SelectSingleNode("//h2[@id='interfaces']")?.InnerText;
            if (h2Node != null && h2Node.Contains("Interfaces"))
            {
                return true;
            }

            // Determine if the page contains the specified content "Enums"
            h2Node = doc.DocumentNode.SelectSingleNode("//h2[@id='enums']")?.InnerText;
            if (h2Node != null && h2Node.Contains("Enums"))
            {
                return true;
            }

            return false;
        }

        static void ExportData(List<string> pages)
        {
            string jsonString = JsonSerializer.Serialize(pages);
            // Console.WriteLine(jsonString);
            File.WriteAllText("../ContentValidation.Test/appsettings.json", jsonString);
        }
    }
}
