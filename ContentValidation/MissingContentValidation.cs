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

        foreach (var row in rows)
        {
            var cells = await row.Locator("td, th").AllAsync();
            foreach (var cell in cells)
            {
                var textContent = await cell.TextContentAsync();
                if (string.IsNullOrWhiteSpace(textContent))
                {
                    res.Result = false;
                    // TODO: res.ErrorMsg = ""
                    return res;
                } 
            }
        }

        await browser.CloseAsync();

        return res;
    }
}
