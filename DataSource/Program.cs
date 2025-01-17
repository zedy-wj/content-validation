using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace DataSource
{
    public class GetDataSource
    {
        private static readonly string PYTHON_SDK_API_URL_PREFIX = "https://learn.microsoft.com/en-us/python/api";
        private static readonly string PYTHON_SDK_API_URL_SUFFIX = "view=azure-python";
        static void Main(string[] args)
        {
            // Default Configuration
            using IHost host = Host.CreateApplicationBuilder(args).Build();

            IConfiguration config = host.Services.GetRequiredService<IConfiguration>();

            string? service = config["ServiceName"];
            string? package = config["PackageName"];

            // Fetch all need to be validated pages in a service/packages.
            List<string> pages = new List<string>();

            pages.Add(GetServiceHomePage(service));

            pages.Add(GetPackagesHomePage(package));

            string apiRefDocPage = GetApiRefDocPage(package);

            pages.Add(apiRefDocPage);

            //Get all the pages in the package that need to be tested.
            GetAllPages(apiRefDocPage, package, pages);

            ExportData(pages);
        }

        static string GetServiceHomePage(string? serviceName)
        {
            serviceName = serviceName?.ToLower().Replace(" ", "-");
            return $"{PYTHON_SDK_API_URL_PREFIX}/overview/azure/{serviceName}?{PYTHON_SDK_API_URL_SUFFIX}";
        }

        static string GetPackagesHomePage(string? packageName) {
            packageName = packageName?.ToLower().Replace("azure-", "");
            return $"{PYTHON_SDK_API_URL_PREFIX}/overview/azure/{packageName}-readme?{PYTHON_SDK_API_URL_SUFFIX}";
        }

        static string GetApiRefDocPage(string? packageName)
        {
            return $"{PYTHON_SDK_API_URL_PREFIX}/{packageName}/{packageName?.ToLower().Replace("-",".")}?{PYTHON_SDK_API_URL_SUFFIX}";
        }

        static void GetAllPages(string apiRefDocPage, string? packageName, List<string> links)
        {
            HtmlWeb web = new HtmlWeb();
            var doc = web.Load(apiRefDocPage);
            var h1Node = doc.DocumentNode.SelectSingleNode("//h1")?.InnerText;

            //The recursion terminates when there are no valid sub pages in the page or when all package links have been visited.
            if (!string.IsNullOrEmpty(h1Node) && h1Node.Contains("Package"))
            {
                var aNodes = doc.DocumentNode.SelectNodes("//td/a");

                if (aNodes != null)
                {
                    foreach (var node in aNodes)
                    {
                        string href = $"{PYTHON_SDK_API_URL_PREFIX}/{packageName}/" + node.Attributes["href"].Value;

                        if (!links.Contains(href))
                        {
                            links.Add(href);

                            //Call GetAllLinks method recursively for each new link.
                            GetAllPages(href, packageName, links);
                        }
                    }
                }
            }
        }

        static void ExportData(List<string> pages)
        {
            string jsonString = JsonSerializer.Serialize(pages);
            Console.WriteLine(jsonString);
            File.WriteAllText("../ContentValidation.Test/appsettings.json", jsonString);
        }
    }
}