using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace UtilityLibraries;

public class UnnecessarySymbolsValidation : IValidation
{
    private IPlaywright _playwright;

    public UnnecessarySymbolsValidation(IPlaywright playwright)
    {
        _playwright = playwright;
    }

    public async Task<TResult> Validate(string testLink)
    {
        var errorList = new List<string>();
        var res = new TResult();
        var browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync();
        await page.GotoAsync(testLink);
        var paragraphs = await page.Locator("p").AllInnerTextsAsync();
        var tableContents = new List<string>();
        var tableCount = await page.Locator("table").CountAsync();
        var codeBlocks = await page.Locator("code").AllInnerTextsAsync();

        if (paragraphs != null)
        {
            for (int i = 0; i < paragraphs.Count; i++)
            {
                var paragraph = paragraphs[i];
                var paragraphMatches = Regex.Matches(paragraph, @"[\[\]<>&]|/{3}");

                foreach (Match match in paragraphMatches)
                {
                    errorList.Add($"Paragraph no.{i + 1} contains unnecessary symbol: {match.Value} in text: {paragraph}\n");
                }
            }
        }

        for (int i = 0; i < tableCount; i++)
        {
            var tableContent = await page.Locator("table").Nth(i).InnerHTMLAsync();
            tableContents.Add(tableContent);
        }

        for (int i = 0; i < tableContents.Count; i++)
        {
            var tableContent = tableContents[i];
            var tagMatches = Regex.Matches(tableContent, @"<\/\w+>\s*&gt;\s*<\/\w+>|~");
            foreach (Match match in tagMatches)
            {
                errorList.Add($"Table no.{i + 1} contains unnecessary symbol: {match.Value}\n");
            }
        }

        if (codeBlocks != null)
        {
            for (int i = 0; i < codeBlocks.Count; i++)
            {
                var codeBlock = codeBlocks[i];
                var tildeMatches = Regex.Matches(codeBlock, @"~");
                foreach (Match match in tildeMatches)
                {
                    errorList.Add($"Code block no.{i + 1} contains unnecessary symbol: {match.Value}\n");
                }
            }
        }

        if (errorList.Count != 0){
            res.Result = false;
            res.ErrorMsg = string.Join(",", errorList);
        }
        
        await browser.CloseAsync();

        return res;
    }
}