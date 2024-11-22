using Microsoft.Playwright;

namespace UtilityLibraries;

public class MissingContentValidation: IValidationNew
{
    private IPlaywright _playwright;

    public MissingContentValidation(IPlaywright playwright)
    {
        _playwright = playwright;
    }

    public async Task<TResultNew> Validate(string testLink)
    {
        //Create a browser instance.
        var browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync();
        await page.GotoAsync(testLink);
        var res = new TResultNew();

        //Fetch all <table> tags and <div> tags containing data-heading-level
        var tableLocator = page.Locator("table");
        var rows = await tableLocator.Locator("tr").AllAsync();
        var headingDivs = await page.Locator("div.heading-wrapper[data-heading-level]").AllAsync();
        var errorMessages = "";

        //Fetch all <tr> tags in the table tag.
        foreach (var row in rows)
        {
            var cells = await row.Locator("td, th").AllAsync();

            //Loop through each <tr> tag and get the 'href' of the <div> tag closest to it.
            foreach (var cell in cells)
            {
                var textContent = await cell.TextContentAsync();

                var tdBoundingBox = await cell.BoundingBoxAsync();
                string specificAnchorHref = string.Empty;

                // Check if the bounding box is valid.
                if (tdBoundingBox != null && tdBoundingBox.Width > 0 && tdBoundingBox.Height > 0)
                {
                    double tdBottom = tdBoundingBox.Y + tdBoundingBox.Height;

                    // Iterate through headingDivs and find the nearest <div> tag.
                    foreach (var divLocator in headingDivs)
                    {
                        var divBoundingBox = await divLocator.BoundingBoxAsync();

                        // Checks if the bounding box of the current <div> tag is valid and precedes the current <p> tag.
                        if (divBoundingBox != null && divBoundingBox.Y + divBoundingBox.Height < tdBottom)
                        {
                            var anchorLocators = await divLocator.Locator("a").AllAsync();
                            if (anchorLocators.Count > 0)
                            {
                                specificAnchorHref = await anchorLocators[^1].GetAttributeAsync("href");
                            }
                        }
                    }
                }

                //Check if there is an empty data cell, if so, return the nearest link.
                if (string.IsNullOrWhiteSpace(textContent))
                {
                    res.Result = false;
                    res.NumberOfOccurrences += 1;
                    errorMessages += $"\n{res.NumberOfOccurrences}." + $" {testLink}+{specificAnchorHref}";
                } 
            }
        }
        res.ErrorLink = $"\nError Link: {testLink}\n";
        res.ErrorInfo = "Error Info: Some cells in the table are missing content.\n";
        res.LocationsOfErrors.Add("Locations of Errors:" + errorMessages);

        await browser.CloseAsync();

        return res;
    }
}
