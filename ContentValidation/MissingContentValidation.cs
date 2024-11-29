using Microsoft.Playwright;

namespace UtilityLibraries;

public class MissingContentValidation : IValidation
{
    private IPlaywright _playwright;

    public MissingContentValidation(IPlaywright playwright)
    {
        _playwright = playwright;
    }

    public async Task<TResult> Validate(string testLink)
    {

        var res = new TResult();
        var errorList = new List<string>();

        // Create a browser instance.
        var browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync();
        await page.GotoAsync(testLink);

        // Fetch all <td>,<th> tags in the page.
        var cellElements = await page.Locator("td,th").AllAsync();
        foreach (var cell in cellElements)
        {
            var cellText = (await cell.InnerTextAsync()).Trim();

            if (string.IsNullOrEmpty(cellText))
            {
                // Fetch the first <a> href before the current cell.
                var aLocator = cell.Locator("xpath=//preceding::a[@class='anchor-link docon docon-link'][1]");
                var href = await aLocator.GetAttributeAsync("href");
                string anchorLink = "No anchor link found, need to manually search for empty cells on the page.";
                if (href != null)
                {
                    anchorLink = testLink + href;
                }
                errorList.Add(anchorLink);
            }
        }

        var formattedList = errorList
            .GroupBy(item => item)
            .Select((group,Index) => $"{Index + 1}. Appears {group.Count()} times , location : {group.Key}")
            .ToList();

        if (errorList.Count > 0)
        {
            res.Result = false;
            res.ErrorLink = testLink;
            res.ErrorInfo = "Some cells in the table are missing content";
            res.NumberOfOccurrences = errorList.Count;
            res.LocationsOfErrors = formattedList;
        }

        await browser.CloseAsync();

        return res;
    }
}