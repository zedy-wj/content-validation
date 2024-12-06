using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace UtilityLibraries;

public class UnnecessarySymbolsValidation : IValidation
{
    private IPlaywright _playwright;

    public  HashSet<string> valueSet = new HashSet<string>();

    public  List<string> errorList = new List<string>();

    public  TResult res = new TResult();

    public UnnecessarySymbolsValidation(IPlaywright playwright)
    {
        _playwright = playwright;
    }

    public async Task<TResult> Validate(string testLink)
    {

        //Create a browser instance.
        var browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync();
        await page.GotoAsync(testLink);


        //Fetch all text content to store in a list. Use regular expressions to find matching unnecessary symbols.
        string htmlContent = await GetHtmlContent(page);
        ValidateHtmlContent(htmlContent);

        //Fetch all 'code' content to store in a list. Use regular expressions to find matching unnecessary symbols.
        var codeBlocks = await page.Locator("code").AllInnerTextsAsync();
        ValidateCodeContent(codeBlocks);



        var formattedList = errorList
            .GroupBy(item => item)
            .Select((group, Index) => $"{Index + 1}. Appears {group.Count()} times ,  {group.Key}")
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

    private void ValidateCodeContent(IReadOnlyList<string> codeBlocks)
    {
        string includePattern = @"~";

        foreach (string codeBlock in codeBlocks)
        {
            string[] lines = codeBlock.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                var matchCollections = Regex.Matches(line, includePattern);
                foreach (Match match in matchCollections)
                {
                    valueSet.Add(match.Value);
                    errorList.Add($"Unnecessary symbol: {match.Value} in code: {line}");
                }
            }
        }
    }

    private void ValidateHtmlContent(string htmlContent)
    {
        // Includes [ ] < > & ~ /// symbols
        string includePattern = @"[\[\]<>&~]|/{3}";

        // Excludes <, > used for comparison. e.g. a > b
        string excludePattern1 = @"(?<=\w\s)[<>](?=\s\w)";

        // Excludes [ ] symbols but not if there are spaces in between. e.g. list[dict] [ada dsafdasf vcxzv]
        string excludePattern2 = @"\[(?!.*\s).*?\]";

        string[] lines = htmlContent.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            var matchCollections = Regex.Matches(line, includePattern);
            foreach (Match match in matchCollections)
            {
                if (match.Value.Equals("<") || match.Value.Equals(">"))
                {
                    if (Regex.IsMatch(line, excludePattern1))
                    {
                        continue;
                    }
                    if (line.Contains("<xref"))
                    {
                        continue;
                    }
                    // Excludes => and -< -> symbols
                    int i = match.Index - 1;
                    if (i >= 0 && (line[i] == '=' || line[i] == '-'))
                    {
                        continue;
                    }
                }

                if ((match.Value.Equals("[") || match.Value.Equals("]")))
                {
                    if (Regex.IsMatch(line, excludePattern2))
                    {
                        continue;
                    }
                    if (line.Contains("<xref"))
                    {
                        continue;
                    }
                }

                valueSet.Add(match.Value);
                errorList.Add($"Unnecessary symbol: {match.Value} in text: {line}");
            }
        }
    }

    private async Task<string> GetHtmlContent(IPage page)
    {
        var contentElement = page.Locator(".content");

        await contentElement.EvaluateAsync(@"(element) => {
            const codeElements = element.querySelectorAll('code');
            codeElements.forEach(code => code.remove());
        }");

        var text = await contentElement.InnerTextAsync();
        return text;
    }
}