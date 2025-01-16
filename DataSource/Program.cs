using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using Microsoft.Playwright;
using System;
using PublicTool;

namespace DataSource
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            // Default Configuration
            // using IHost host = Host.CreateApplicationBuilder(args).Build();

            // IConfiguration config = host.Services.GetRequiredService<IConfiguration>();

            // string? langues = config["Langues"];
            // string? service = config["ServiceName"];
            // string? packagename = config["PackageName"];
            string? langues = "python";
            string? service = "Cognitive Services";
            string? packagename = "azure-ai-formrecognizer";

            List<string> allpages = new List<string>();

            if (langues == "Python" || langues == "python")
            {
                PythonTools.Run(service, packagename, allpages);
            }
            if (langues == "Java" || langues == "java")
            {
                await JavaTools.Run(packagename, allpages);
            }

            foreach (var page in allpages)
            {
                Console.WriteLine(page);
            }
            // PublicTools.ExportData(allpages);

            // host.RunAsync();
        }
    }
}
