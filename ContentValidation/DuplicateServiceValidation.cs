using Microsoft.Playwright;
using UtilityLibraries;
using static System.Net.Mime.MediaTypeNames;

namespace ContentValidation;

public class DuplicateServiceValidation : IValidation
{
    private IPlaywright _playwright;

    public DuplicateServiceValidation(IPlaywright playwright)
    {
        _playwright = playwright;
    }

    public async Task<TResult> Validate(string testLink)
    {
        var browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync();
        var res = new TResult();
        var set = new HashSet<string>();
        var list = new List<string>();

        await page.GotoAsync(testLink);
        await page.WaitForSelectorAsync("li.border-top.tree-item.is-expanded");

        var parentLi = await page.QuerySelectorAsync("li.border-top.tree-item.is-expanded");
        var liElements = await parentLi.QuerySelectorAllAsync("ul.tree-group > li[aria-level='2']");

        foreach (var element in liElements)
        {
            var text = await element.InnerTextAsync();
            if (text != "Overview")
            {
                if (!set.Add(text))
                {
                    res.Result = false;
                    list.Add(text);
                }
                
            }
        }
        res.ErrorMsg += testLink + "has duplicate service at " + string.Join(",", list);

        await browser.CloseAsync();

        return res;
    }
}



