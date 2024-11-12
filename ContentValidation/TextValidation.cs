using Microsoft.Playwright;

namespace UtilityLibraries;

public class TextValidation
{
    private IPlaywright _playwright;

    public TextValidation(IPlaywright playwright)
    {
        _playwright = playwright;
    }
    public (bool Result, string? ErrorMsg) FindDuplicateService(string text)
    {
        var errorList = new List<string>();
        //TODO
        return (true, string.Join(",", errorList));
    }

    public (bool Result, string? ErrorMsg) FindGarbledText(string text)
    {
        var errorList = new List<string>();
        //TODO
        return (true, string.Join(",", errorList));
    }

    public async Task<bool> FindEmptyTable(string testLink)
    {
        var browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync();
        await page.GotoAsync(testLink);

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
                    return false;
                } 
            }
        }

        await browser.CloseAsync();

        return true;
    }
}
