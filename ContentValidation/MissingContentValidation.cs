using Microsoft.Playwright;

namespace UtilityLibraries;

public class MissingContentValidation: IValidation
{
    private IPlaywright _playwright;

    public MissingContentValidation(IPlaywright playwright)
    {
        _playwright = playwright;
    }

    public async Task<TResult> Validate(string testLink)
    {
        var browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync();
        await page.GotoAsync(testLink);
        var res = new TResult();

        var tableLocator = page.Locator("table");
        var rows = await tableLocator.Locator("tr").AllAsync();

        var headingDivs = await page.Locator("div.heading-wrapper[data-heading-level]").AllAsync();
        var errorMessages = new List<string>();

        foreach (var row in rows)
        {
            var cells = await row.Locator("td, th").AllAsync();
            foreach (var cell in cells)
            {
                var textContent = await cell.TextContentAsync();

                var tdBoundingBox = await cell.BoundingBoxAsync();
                string specificAnchorHref = string.Empty;

                if (tdBoundingBox != null && tdBoundingBox.Width > 0 && tdBoundingBox.Height > 0)
                {
                    double tdBottom = tdBoundingBox.Y + tdBoundingBox.Height;

                    foreach (var divLocator in headingDivs)
                    {
                        var divBoundingBox = await divLocator.BoundingBoxAsync();

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

                if (string.IsNullOrWhiteSpace(textContent))
                {
                    res.Result = false;
                    var errorMessage = $"{textContent} " + $"\nLink : {testLink}+{specificAnchorHref}";
                    errorMessages.Add(errorMessage);
                } 
            }
        }
        res.ErrorMsg = string.Join("",errorMessages);
        await browser.CloseAsync();

        return res;
    }
}