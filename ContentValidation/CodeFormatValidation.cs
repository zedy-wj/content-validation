using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace UtilityLibraries;

public class CodeFormatValidation : IValidation
{
    private IPlaywright _playwright;

    public CodeFormatValidation(IPlaywright playwright)
    {
        _playwright = playwright ?? throw new ArgumentNullException(nameof(playwright));
    }

    public async Task<TResult> Validate(string testLink)
    {
        //Create a browser instance.
        var browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync();
        await PlaywrightHelper.GotoageWithRetriesAsync(page, testLink);

        var res = new TResult();
        var errorList = new List<string>();

        //Get all code content in the test page.
        var codeElements = await page.Locator("code").AllAsync();

        //Check if there are wrong code format.
        foreach (var element in codeElements)
        {
            var codeText = await element.InnerTextAsync();

            // Check for unnecessary space before import
            var matches = Regex.Matches(codeText, @"^ import\s+[a-zA-Z_]\w*([.][a-zA-Z_]\w*)+;$", RegexOptions.Multiline);

            foreach (Match match in matches)
            {
                errorList.Add($"Unnecessary space before import: {match.Value}");
            }
        }

        if(errorList.Count > 0)
        {
            res.Result = false;
            res.ErrorLink = testLink;
            res.NumberOfOccurrences = errorList.Count;
            res.ErrorInfo = "Have Incorrect Code Format: " + string.Join(",", errorList);
        }

        await browser.CloseAsync();

        return res;
    }
}



