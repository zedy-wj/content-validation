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
        private static readonly string SDK_API_REVIEW_URL_BASIC = "https://review.learn.microsoft.com/en-us/";
        static async Task Main(string[] args)
        {
            // Default Configuration
            using IHost host = Host.CreateApplicationBuilder(args).Build();

            IConfiguration config = host.Services.GetRequiredService<IConfiguration>();

            string? package = config["ReadmeName"];
            string? language = config["Language"];
            string branch = config["Branch"]!;
            
            using (HttpClient client = new HttpClient())
            {
                // 设置请求头，包括 cookie
                client.DefaultRequestHeaders.Add("Cookie", "MC1=GUID=49caf7a7839a4e00b33b983ae7d86f68&HASH=49ca&LV=202409&V=4&LU=1726647915836; MUID=35577CF9297169BE124A6807289B68BA; _clck=wf4mqd%7C2%7Cfq1%7C0%7C1749; MicrosoftApplicationsTelemetryDeviceId=855c0671-eb61-4979-8f37-7d14bfd53682; MSFPC=GUID=49caf7a7839a4e00b33b983ae7d86f68&HASH=49ca&LV=202409&V=4&LU=1726647915836; MSCC=NR; at_check=true; mbox=PC#7bb65244851842a1af61ee58f790cb04.32_0#1774562965|session#e9f3690f9e464640af0354cf530776c8#1740378127; AMCVS_EA76ADE95776D2EC7F000101%40AdobeOrg=1; AMCV_EA76ADE95776D2EC7F000101%40AdobeOrg=1585540135%7CMCIDTS%7C20144%7CMCMID%7C23374112602575781132210532747731934712%7CMCAAMLH-1740981068%7C3%7CMCAAMB-1740981068%7CRKhpRz8krg2tLO6pguXWp5olkAcUniQYPHaMWWgdJ3xzPWQmdj0y%7CMCCIDH%7C-818852712%7CMCOPTOUT-1740383468s%7CNONE%7CMCAID%7CNONE%7CvVersion%7C4.4.0; _uetsid=589e52c0f27311ef918e3f8b0e803fb6|1jg7mh1|2|ftp|0|1881; _uetvid=d87e96b0406f11ec8b844b02184dc581|17uornf|1740376270298|1|1|bat.bing.com/p/insights/c/k; .AspNetCore.Cookies=CfDJ8HtqtwpXewxMhrJ2ckzYMLSOuSASX2kbsjGnPoauyDon275eCts0dWDQixbcR8UEVcHl06sQlzHV4mXcvc101hCBViMPV8D7ylOsxzPoU3IgM9p8eu2UCs6D1y4ddBxRmDYiUciOBNvqqqAhYhSUPA4Xu6H38TXVeIKfjjNuAiCuUqnbG9EzDe98bdij1YbM8IjrtV0zNIpX6fYWeDY9s13ygy6CAQbxQuMFjfoAMTgViGl6KZzzZHo75BE0r7tWSsMWMW1ia0vLM1SXZ5Ne3idPG7Ji9xZrnQcZY7Hcy_BnxORjhjTvmmvDzjny7TLg1mXvBU5LJNwLDoReuPs4RXRZtrh6EBWo3f3A1e_osz5rxP5a2WiWBZ7abEapK1ArT3m08x5ypsJYixJPGmOIRw4vcrlwF1T5AoEyXSt7oI68u1gzzzQH2_NW6Kx46dzkjYUD62vQOwEjo5dEES5PMWGpixnpoH3kvimu4oG71npprY9pxAlt2evtnq2kqk22eLUfnVZbe7yZV67FBPZ4X6Qhu2bckRfCgPCBnvyOTUzHuioo-KJLYv9TLiTzCizJQXtl6LqBDKg9mhhnB7QAzUjMV98Z0aV5F0DrraDN_Hwg7eDFXJcg7rJdfw2DcbewmnXo80q4I9ub7y6-l7W76eT-3JsTKRI27QVSJnjX2LUV5e7sl8Xtof9tY_8lblwyzrblxJIGn7TjFpQMoNLepP7kwfkz_BRZ72214ZbpSH_gZ1nU0XenEM8luN9h4Fntx4jAeCuE6CX76dLlOucWuMQrPJQ9G_RWKer5mjwPLVF0QegbbdUMPUHEH3vLohOeYEXjURMG2_M7BG7kLpFESJ9whKH3SI8t51TuI5weki1fdX2n1xFZLNcoLAOug6Hs-ctGbwOYvukJtfmQHd9QooNrmRrVUvgQctVdx_7Bpi5fwbHeNzI0aO3h_xG1TC6oj9WzdAJtN6u1f47TeX9rF96CLOU3BKL7n4fiDCJMFvhPMDFXG1kFyJPdJEki83V0NKg2DS1kzCbzeSAU8aUfPZJOI36fGj0KpvsWr1ko1dqyEyBC6YV0GwllLJNE8sI89SFFn3fpv2fmEo0nU5OBR3T2QYYCIc4bCiTbnSZN4ZqvLJro6LVdHdxnAM7XZy43YI6qRJfVBfOy_3Uy4w7VKYvyQAUjdUU4jheE9QdW6AN8cWTwSPgHdOyr7r1A2iQcbNOTDnzkeFuVsmUfr2CSjGymLS7C05Cpj_HOFsvXcnf2dhNkTGEH9kVGDi9Bxc4hKYne-isuuUs-TDKGbtZb4zWegrV4fNix94YCVXjkBaeVfu5Ypca_JfMs3-n8w4gT1qeaSbw93ND4TTAMW-dZidJuN0MNqg5OHdhxOR-b6oqki2ddGTHjYxLHZFKFepDVpNqIHIstVDB6ySsbfCe_H80oDxhVxWouPCL9ItwduJlwYfxxem1FM47Qa4URBorIz-wfQkCaKMl0rrrf5u85pVJZC1la4kHggukiTfFFn7FbB7AFVi5G8ZdmOY6yyyLhBAt3UK8krfohp2h6kYOQoQn0-GwDiQfvuQw7jWaVrfd9wyJJ7eagkxLN1GrOcC8vSmTT4m9kGBtoseLTJw8NS0I81HiPj3qfGuTEV6MT-eplesUNr33xfxDeaGHj16mbOmclJ4omx9ih9HkIkoFYPpCZSIy0vQ8iu2gMWf5DLBXOx6qbgtLfjcQ8EWCLpT0JNKLhFI_of2Tzea1e7qHnV9v82Su6DuwnwlihkP57LNiy1XqPJVbaJO8T7rHG7a7vUI3f0wY-CZcjUFLZtHQ9LvpKCYH3sKSlt_YbFitm0swwVD_jIwqkwSioNBa3Y0c-xEqrQHdZVGXgWpKI7QT2CIgOdqmEs_pEFfg9x30BPP5rG27LY_4nowZcFrR2WsWBQBKOUUCuaGCFZi29gG-LXvNge9as1Xpe7g_DV709GiX3thhKydQJXgVcPIFZ-QdPwJJT4N1ttyUOgLFzglBKPjfhEGd42bn_GoO4F6JuhmbCucFmAvHf2_ste7h0QaeZ8puWhJNrP3DD1GTi38emNojlzS3Z270k705KtNZd3hyxM-wxLdb2izwl2qTgGKq4HQWKWakA7sVUrErzQw2sdhzUAS4iqTm4ka2JD79ReGR4_WB3VaeyC3Qoy9tQ1xO_7JGpf1fcy75SmnZbty9OVbfAH7gP_9sep5Lo2tcJyKbG88J0ZYu27DnaxqJKRQ4w5O5_QbiMQeLRGdppoIS9Q8BiWH5Hv271aR-LJsw1eos66ERqBDR5_BCO4oKbmFuBt828UXEvgnjhtbyyRVCHp5XreNXRZOXPpprMbu1aZDNy2ENWV7tJLoIz_bKLj0BHMIqgnjZEo3I9Dz7QIBoaoyb9CNI1jybRsjE4H9L2HscpPAy0yfwdLMFH4s8T5CyLqUkbwIoZ6mKjLcbu3CqvccyVJKfV-W1P3bUWKYxe8mHDe9f1VRndSF3UaWj2ndAyZpXXdDKArr9dkZfZ0uKTieogzLpxkwJZ-EGdMk0KoZae15IAIZoI9ViLAln0V02Lp6VCVYhgINOoYcf5OVvyucpIBsXtjGgAoyJ65o9Q0BZ4RRDOh8Xk2uwrsz-_KH8VKqfQQHtzDesp3jlyDaCGmJZJYNe0qwKTWsk5Tt96xrc4etFWSKLNCTKJ_ImByO-Sfs_G10V25mGLm9W61yVVAHxcAifpE1EiMPLQEvapaazPMXGSjnvt0AITLnYtFvanyxIy-MJLnUyILFsiolfT8NLp6u2GUIufya8UuNKNl7zCuDZK2JGEMn4tZDDKsW7SooMVMr010278wRZzjqEFVWNLh6fSW72lvmKsuEmxQKip5W34XM3sEARdTuF0laWvSuHOlL10lJbJ2xaasgXKKd3VDLtklvZbL_Z9NLdxPhNbjiWRg964jLRXlFxrPSsfserQUhZ3pT4u1pNFCo8dFrh_c5PYx-CiVLONyUVrQ4uwDiH1KoGMSAPdldYiLddNUkxgMa_X8BWGmEjP0BMaf3KxP26npPMI7pYr1qA2iQEJWXAs4lSrdPwJzDfn5r5aHuwxeI89rGtFMQ7sJKpD1rysdzSIIvKHZ4G6aWtBQiL9DzwWjX4EUVRaRxxoGllQYzRhShQ9zpQhI5FKClaGeAOkYAmFmTP68moaMaboDZj3qAiT0qgABV-SMCQIt0vlRDdo0C4MltBWRhoJHU4tDhG9teHOZ_5LYtDNcQowG4m2rtfnIqxq5hag2kNfMlsb-FxquHqa6_5VEVyJBrUZdLPAhCkv8eXP2JxM7Q_qQVfWll-pJHcrHsvl-oZRtciG874yuXqFLOE_xPTDcDG7nqjk1Cg0Ypzae-QZlA625tABfD-RFp5cW1AXg9ZNhdUvLbQgHC867AQ4aJMEvvLoPqa7d7Gw4hxMyZZTr9fUa9dtesbwlinahWngSGv4LRwMH0XBBSLtaUSNuZow_V7f82Wx0adxRwpAR1VwzWjVQe_L0KuLfQi4W97JACPJ9pkwMbIoAJMtX52IzB0nZL1mF-Tp7_XfjyiSyXMFG94wTB9QMLgEZTH642LKRNfz4lc6tQo-_Wuq76E0LYw0sF5PWIg; LEARN_INTERNAL=eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6IjA2OWM1YTJmMjFmMDQyNWM5NGUxYmJjZmRhZTM4MDdjIn0.eyJpbnRlcm5hbF9mZWF0dXJlX2FjY2VzcyI6dHJ1ZSwiaXNzIjoiaHR0cHM6Ly9kb2NzLm1pY3Jvc29mdC5jb20iLCJhdWQiOiJkb2NzLnNlcnZpY2VzIiwibmJmIjoxNzQwMzgzMjY1LCJleHAiOjE3NDA5ODgwNjV9.UuUQTMZwU4G3ztmKCqRtq8zY9e9F8xhzqJRcnJ6VQuOp-kNZ5T4MRxivl8_prt8zh_1lmIxukzbNX2yvvrn1SMiUcXSftwMZ234LwP3vyV9ZlZN2NoBcIm-3MV_8RGDUtjxDp6ETuIfPzs3j9_Yn357OAMbkC4Vtr-byb5C-WkE7cixJqe3U-Fqzr75nz_sbmk8diwukS5rOMJOACnP6RwVRLrf3My4ppH4nZSa1osIHYyCgwtGrWgD2nF3k4HEP6jf2JZkGJUTfFtsTRIUA-mClAeKfsNNd0thIP6NY8Sgng96kToXoPHCAZKF1CKXZ5QheyO6rzlIlA3urHwq6Jw; CONTENT_BRANCH=main; MS0=04eb3ed505db4b969a040417f028cdd6; ai_session=eXwBNR91NVWnQo2CD0MUyn|1740386960538|1740389823890"); // 替换为实际的 cookie
    
                // 发送 GET 请求到需要身份验证的 URL
                HttpResponseMessage response = await client.GetAsync("https://review.learn.microsoft.com/en-us/python/api/overview/azure/appconfiguration-readme?view=azure-python&branch=main");
    
                // 确保请求成功
                response.EnsureSuccessStatusCode();
    
                // 读取响应内容
                string responseBody = await response.Content.ReadAsStringAsync();
    
                // 使用 HtmlAgilityPack 解析 HTML
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(responseBody);
    
                // 现在你可以使用 HtmlAgilityPack 的 API 来操作或查询 HTML 文档了
                // 例如，获取某个元素的文本内容
                var someElement = htmlDoc.DocumentNode.SelectSingleNode("//h1");
                if (someElement != null)
                {
                    Console.WriteLine(someElement.InnerText);
                }
            }
            
            string? overviewUrl = GetLanguagePageOverview(language, branch);

            List<string> pages = new List<string>();
            List<string> allPages = new List<string>();
            string pagelink = $"{overviewUrl}/{package}?branch={branch}";

            await GetAllChildPage(pages, allPages, pagelink);

            ExportData(allPages);
        }

        static string GetLanguagePageOverview(string? language, string branch)
        {
            language = language?.ToLower();
            if(branch != "main")
            {
                return $"{SDK_API_URL_BASIC}/{language}/api/overview/azure/";
            }
            else{
                return $"{SDK_API_REVIEW_URL_BASIC}/{language}/api/overview/azure/";
            }
        }

        static async Task GetAllChildPage(List<string> pages, List<string> allPages, string pagelink)
        {
            // Launch a browser
            var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
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

                await page.GotoAsync(pagelink, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle
                });
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
                    int lastSlashIndex = pa.LastIndexOf('/');
                    string baseUri = pa.Substring(0, lastSlashIndex + 1);
                    allPages.Add(pa);
                    GetAllPages(pa, baseUri, allPages);
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
                var aNodes = doc.DocumentNode.SelectNodes("//td/a | //td/span/a");

                if (aNodes != null)
                {
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