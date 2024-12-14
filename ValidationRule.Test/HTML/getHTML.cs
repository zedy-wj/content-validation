// using System;
// using System.IO;
// using System.Threading.Tasks;
// using Microsoft.Playwright;

// namespace PlaywrightExample
// {
//     class Program
//     {
//         static async Task Main(string[] args)
//         {
//             // 启动 Playwright
//             var playwright = await Playwright.CreateAsync();
//             var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
//             var page = await browser.NewPageAsync();

//             // 打开页面
//             var url = "https://learn.microsoft.com/en-us/python/api/azure-ai-formrecognizer/azure.ai.formrecognizer.aio.formrecognizerclient?view=azure-python"; // 替换为你想抓取的 URL
//             await page.GotoAsync(url);

//             // 获取页面 HTML
//             var htmlContent = await page.ContentAsync();

//             // 保存 HTML 到本地文件
//             string filePath = "page.html";
//             await File.WriteAllTextAsync(filePath, htmlContent);

//             // 输出保存路径
//             Console.WriteLine($"HTML saved to {filePath}");

//             // 关闭浏览器
//             await browser.CloseAsync();
//         }
//     }
// }


