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
        var errorList = new List<string>();
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
                    errorList.Add($"{errorList.Count+1}. Paragraph no.{i + 1} contains unnecessary symbol: {match.Value} in text: {paragraph}\n");
                }
            }
        }

        //Fetch all 'table' content to store in a list. Use regular expressions to find the occurrence of unnecessary symbols between two closing tags.
        var tableContents = new List<string>();
        var tableCount = await page.Locator("table").CountAsync();

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
                var value = match.Value;
                if (!value.Equals("~"))
                {
                    value = "<";
                }
                valueSet.Add($"{value}");
                errorList.Add($"{errorList.Count+1}. Table no.{i + 1} contains unnecessary symbol: {value} \n");
            }
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
                    errorList.Add($"{errorList.Count+1}. Code block no.{i + 1} contains unnecessary symbol: {match.Value}\n");
                }
            }
        }

        //Log the error message in the report.
        if (errorList.Count != 0){
            res.Result = false;
            res.ErrorLink = testLink;
            res.ErrorInfo = $"Unnecessary symbols found: {string.Join(" ",valueSet)}";
            res.NumberOfOccurrences = errorList.Count;
            res.LocationsOfErrors.Add("\nLocations of Errors:\n" + string.Join("", errorList));
        }
        
        await browser.CloseAsync();

        return res;
    }
}