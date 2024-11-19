using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace UtilityLibraries
{
    public class GarbledTextValidation : IValidation
    {
        private readonly IPlaywright _playwright;

        public GarbledTextValidation(IPlaywright playwright)
        {
            _playwright = playwright;
        }

        public async Task<TResult> Validate(string testLink)
        {
            var browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var page = await browser.NewPageAsync();
            await page.GotoAsync(testLink);
            var res = new TResult();

            // 获取所有 <p> 标签和包含 data-heading-level 的 <div> 标签
            var pLocators = await page.Locator("p").AllAsync();
            var headingDivs = await page.Locator("div.heading-wrapper[data-heading-level]").AllAsync();
            var errorMessages = new List<string>(); // 用于存储所有错误消息

            // 遍历每个 <p> 标签
            foreach (var pLocator in pLocators)
            {
                var text = await pLocator.TextContentAsync();
                string specificAnchorHref = string.Empty; // 存储最近的链接

                // 获取当前 <p> 标签的边界框
                var pBoundingBox = await pLocator.BoundingBoxAsync();

                // 检查边界框是否有效
                if (pBoundingBox != null && pBoundingBox.Width > 0 && pBoundingBox.Height > 0)
                {
                    // 计算 p 标签的底部位置
                    double pBottom = pBoundingBox.Y + pBoundingBox.Height;

                    // 遍历 headingDivs 查找最近的 <div> 标签
                    foreach (var divLocator in headingDivs)
                    {
                        // 获取当前 <div> 标签的边界框
                        var divBoundingBox = await divLocator.BoundingBoxAsync();

                        // 检查当前 <div> 标签的边界框是否有效并在当前 <p> 标签之前
                        if (divBoundingBox != null && divBoundingBox.Y + divBoundingBox.Height < pBottom)
                        {
                            // 从最近的 <div> 提取链接
                            var anchorLocators = await divLocator.Locator("a").AllAsync();
                            if (anchorLocators.Count > 0)
                            {
                                specificAnchorHref = await anchorLocators[^1].GetAttributeAsync("href");
                            }
                        }
                    }
                }

                // 检查文本是否匹配乱码模式
                if (Regex.IsMatch(text, @":[\w]+(?:\s+[\w]+){0,2}:"))
                {
                    res.Result = false;
                    var errorMessage = $" \n{text} " + $"\nLink : {testLink}" + $"{specificAnchorHref}\n";
                    errorMessages.Add(errorMessage);
                }
            }
            res.ErrorMsg = string.Join("",errorMessages);

            await browser.CloseAsync();
            return res;
        }
    }
}
