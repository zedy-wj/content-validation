using Microsoft.Playwright;

namespace UtilityLibraries;

public class ExtraLabelValidation : IValidation
{
    private IPlaywright _playwright;

    public ExtraLabelValidation(IPlaywright playwright)
    {
        _playwright = playwright;
    }
    public async Task<TResult> Validate(string testLink)
    {
        var res = new TResult();
        var labelList = new List<string> {
            "<br",
            "<h1",
            "<h2",
            "<h3",
            "<h4",
            "<h5",
            "<h6",
            "<em",
            "<span",
            "<div",
            "<ul",
            "<ol",
            "<li",
            "<table",
            "<tr",
            "<td",
            "<th",
            "<img",
            "<code",
            "<xref",
            "&amp;",
            "&lt",
            "&gt",
            "&quot",
            "&apos"
        };

        var browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync();
        await page.GotoAsync(testLink);

        var text = await page.Locator("html").InnerTextAsync();

        foreach (var label in labelList)
        {

            int index = 0;
            int count = 0;
            while ((index = text.IndexOf(label, index)) != -1)
            {
                count++;
                index += label.Length;
            }

            if (count != 0)
            {
                res.Result = false;
                res.Add("", $"{label}", count.ToString());
            }
        }

        await browser.CloseAsync();

        return res;
    }
}