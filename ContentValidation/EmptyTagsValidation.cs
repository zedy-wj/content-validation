using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UtilityLibraries;

public class EmptyTagsValidation : IValidation
{
    private IPlaywright _playwright;

    public EmptyTagsValidation(IPlaywright playwright)
    {
        _playwright = playwright ?? throw new ArgumentNullException(nameof(playwright));
    }

    public async Task<TResult> Validate(string testLink)
    {
        // Create a browser instance.
        var browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync();
        await PlaywrightHelper.GotoageWithRetriesAsync(page, testLink);

        var res = new TResult();
        var errorList = new List<string>();

        var htmlContent = await page.ContentAsync();

        // Define the regex pattern to match empty <li> tags
        string pattern = @"<li>\s*</li>";

        // Use Regex.Matches to find all matches in the HTML content
        var matches = Regex.Matches(htmlContent, pattern, RegexOptions.Multiline);

        int threshold = 2; 

        if (matches.Count > threshold)
        {
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    errorList.Add(match.Value);
                }
            }

            var formattedList = errorList
                .GroupBy(item => item)
                .Select((group, index) => $"{index + 1}. Appears {group.Count()} times, {group.Key}")
                .ToList();

            res.Result = false;
            res.ErrorLink = testLink;
            res.NumberOfOccurrences = errorList.Count;
            res.ErrorInfo = $"There are too many empty <li> tags. ";
            res.LocationsOfErrors = formattedList;
        }
        
        await browser.CloseAsync();

        return res;
    }
}