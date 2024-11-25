using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace UtilityLibraries;

public class UnnecessarySymbolsValidation : IValidationNew
{
    private IPlaywright _playwright;

    public UnnecessarySymbolsValidation(IPlaywright playwright)
    {
        _playwright = playwright;
    }

    public async Task<TResultNew> Validate(string testLink)
    {
        var valueSet = new HashSet<string>();
        var res = new TResultNew();

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
                var paragraphMatches = Regex.Matches(paragraph, @"[\[\]<>&]|/{3}");

                foreach (Match match in paragraphMatches)
                {
                    valueSet.Add(match.Value);
                    res.LocationsOfErrors.Add($"{res.LocationsOfErrors.Count+1}. Paragraph no.{i + 1} contains unnecessary symbol: {match.Value} in text: {paragraph}");
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
                res.LocationsOfErrors.Add($"{res.LocationsOfErrors.Count + 1}. Table no.{index + 1} contains unnecessary symbol: {value}");
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
                    res.LocationsOfErrors.Add($"{res.LocationsOfErrors.Count+1}. Code block no.{i + 1} contains unnecessary symbol: {match.Value}");
                }
            }
        }

        if (res.LocationsOfErrors.Count != 0){
            res.Result = false;
            res.ErrorLink = testLink;
            res.ErrorInfo = $"Unnecessary symbols found: {string.Join(",",valueSet)}";
            res.NumberOfOccurrences = res.LocationsOfErrors.Count;
        }
        
        await browser.CloseAsync();

        return res;
    }
}