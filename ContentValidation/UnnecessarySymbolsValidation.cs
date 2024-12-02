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
        var valueSet = new HashSet<string>();
        var res = new TResult();
        var errorList = new List<string>();

        //Create a browser instance.
        var browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });

        //Fetch all 'p' tags content to store in a list. Use regular expressions to find matching target symbols.
        var page = await browser.NewPageAsync();
        await page.GotoAsync(testLink);
        var paragraphs = await page.Locator("p").AllInnerTextsAsync();

        if (paragraphs != null)
        {
            for (int i = 0; i < paragraphs.Count; i++)
            {
                var paragraph = paragraphs[i];
                var paragraphMatches = Regex.Matches(paragraph, @"[\[\]<>&~]|/{3}");

                foreach (Match match in paragraphMatches)
                {
                    valueSet.Add(match.Value);
                    errorList.Add($"Paragraph no.{i + 1} contains unnecessary symbol: {match.Value} in text: {paragraph}");
                }
            }
        }

        //Fetch all 'table' content to store in a list. Use regular expressions to find the occurrence of unnecessary symbols between two closing tags.
        var tableContents = new List<string>();
        var tableCount = await page.Locator("table").CountAsync();
        int index = 0;

        for (int i = 0; i < tableCount; i++)
        {
            var tableContent = await page.Locator("table").Nth(i).InnerHTMLAsync();
            tableContents.Add(tableContent);
        }

        foreach (var tableContent in tableContents)
        {
            var tagMatches = Regex.Matches(tableContent, @"<\/\w+>\s*&gt;\s*<\/\w+>|~");
            foreach (Match match in tagMatches)
            {
                var value = match.Value;
                if (!value.Equals("~"))
                {
                    value = ">";
                }
                valueSet.Add($"{value}");
                errorList.Add($"Table no.{index + 1} contains unnecessary symbol: {value}");
            }
            index++;
        }

        //Fetch all 'code' content to store in a list. Use regular expressions to find matching target symbols.
        var codeBlocks = await page.Locator("code").AllInnerTextsAsync();

        if (codeBlocks != null)
        {
            for (int i = 0; i < codeBlocks.Count; i++)
            {
                var codeBlock = codeBlocks[i];
                var tildeMatches = Regex.Matches(codeBlock, @"~");
                foreach (Match match in tildeMatches)
                {
                    valueSet.Add(match.Value);
                    errorList.Add($"Code block no.{i + 1} contains unnecessary symbol: {match.Value}");
                }
            }
        }

        var formattedList = errorList
            .GroupBy(item => item)
            .Select((group, Index) => $"{Index + 1}. Appears {group.Count()} times , location : {group.Key}")
            .ToList();

        if (errorList.Count > 0)
        {
            res.Result = false;
            res.ErrorLink = testLink;
            res.NumberOfOccurrences = errorList.Count;
            res.ErrorInfo = $"Unnecessary symbols found: {string.Join(",", valueSet)}";
            res.LocationsOfErrors = formattedList;
        }

        await browser.CloseAsync();

        return res;
    }
}