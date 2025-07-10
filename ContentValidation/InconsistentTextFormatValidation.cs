using Microsoft.Playwright;

namespace UtilityLibraries;

public class InconsistentTextFormatValidation : IValidation
{
    private IPlaywright _playwright;

    public InconsistentTextFormatValidation(IPlaywright playwright)
    {
        _playwright = playwright;
    }

    public async Task<TResult> Validate(string testLink)
    {
        //Create a browser instance.
        var browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync();
        await PlaywrightHelper.GotoageWithRetriesAsync(page, testLink);

        var res = new TResult();
        var errorList = new List<string>();
        var errorLocation = new List<string>();

        var hTagsInTd = await page.QuerySelectorAllAsync("td h1, td h2, td h3, td h4, td h5, td h6");

        if (hTagsInTd.Count > 0)
        {
            foreach (var element in hTagsInTd)
            {
                var text = await element.InnerTextAsync();

                string? headerId = null;
                try
                {
                    headerId = await element.GetAttributeAsync("id") ?? string.Empty;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("There no 'id' attribute. " + ex.Message);
                }

                var anchorLink = string.IsNullOrEmpty(headerId) ? $"{testLink}" : $"{testLink}#{headerId}";
                errorList.Add(text);
                errorLocation.Add($" https://learn.{anchorLink} ");
            }
            // Format the error list
            var formattedList = errorLocation
                .GroupBy(item => item)
                .Select((group, index) => $"{index + 1}. Appears {group.Count()} times , location : {group.Key}")
                .ToList();

            if (errorList.Count > 0)
            {
                res.Result = false;
                res.ErrorLink = testLink;
                res.NumberOfOccurrences = hTagsInTd.Count;
                res.ErrorInfo = "Inconsistent Text Format: " + string.Join(",", errorList);
                res.LocationsOfErrors = formattedList;
            }
        }

        await browser.CloseAsync();

        return res;
    }
}