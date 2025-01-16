using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using Microsoft.Playwright;
using System;

namespace PublicTool
{
    public static class PublicTools
    {
        public static readonly string Java_SDK_API_URL_BASIC = "https://learn.microsoft.com/en-us/java/api/";
        public static readonly string PYTHON_SDK_API_URL_PREFIX = "https://learn.microsoft.com/en-us/python/api";

        /// Determine if recursion is required
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

        public static void GetAllPages(string apiRefDocPage, List<string> links, string langues, string? packageName = null)
        {
            HtmlWeb web = new HtmlWeb();
            var doc = web.Load(apiRefDocPage);

            //The recursion terminates when there are no valid sub pages in the page or when all package links have been visited.
            if (IsTrue(apiRefDocPage))
            {
                if(langues == "Python" || langues == "python")
                {
                    var aNodes = doc.DocumentNode.SelectNodes("//td/a");

                    if (aNodes != null)
                    {
                        foreach (var node in aNodes)
                        {
                            string href = $"{PYTHON_SDK_API_URL_PREFIX}/{packageName}/" + node.Attributes["href"].Value;

                            // Determine if the page has been contained
                            if (!links.Contains(href))
                            {
                                links.Add(href);

                                //Call GetAllLinks method recursively for each new link.
                                GetAllPages(href, links, langues, packageName);
                            }
                        }
                    }
                }
                if (langues == "Java" || langues == "java")
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
                                GetAllPages(href, links, langues);
                            }
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

    public static class PythonTools
    {
        public static void Run(string service, string package, List<string> allpages)
        {
            allpages.Add(GetServiceHomePage(service));

            allpages.Add(GetPackagesHomePage(package));

            string apiRefDocPage = GetApiRefDocPage(package);

            allpages.Add(apiRefDocPage);

            //Get all the pages in the package that need to be tested.
            PublicTools.GetAllPages(apiRefDocPage, allpages, "python", package);
        }

        static string GetServiceHomePage(string? serviceName)
        {
            serviceName = serviceName?.ToLower().Replace(" ", "-");
            return $"{PublicTools.PYTHON_SDK_API_URL_PREFIX}/overview/azure/{serviceName}";
        }

        static string GetPackagesHomePage(string? packageName)
        {
            packageName = packageName?.ToLower().Replace("azure-", "");
            return $"{PublicTools.PYTHON_SDK_API_URL_PREFIX}/overview/azure/{packageName}-readme";
        }

        static string GetApiRefDocPage(string? packageName)
        {
            return $"{PublicTools.PYTHON_SDK_API_URL_PREFIX}/{packageName}/{packageName?.ToLower().Replace("-", ".")}";
        }
    }

    public static class JavaTools
    {
        private static readonly string Java_SDK_API_URL_PREFIX = "https://learn.microsoft.com/en-us/java/api/overview/azure/";

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
                    PublicTools.GetAllPages(pa, allpages, "java");
                }
            }
        }
    }
}