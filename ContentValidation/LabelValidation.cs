using Microsoft.Playwright;

namespace UtilityLibraries;

public class LabelValidation
{
    private IPlaywright _playwright;

    public LabelValidation(IPlaywright playwright)
    {
        _playwright = playwright;
    }
    public async Task<TResult> FindExtraLabel(string testLink)
    {
        var errorList = new List<string>();
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
            "<a",
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

            if (text.Contains(label))
            {
                errorList.Add(label);
            }
        }

        if(errorList.Count != 0){
            res.Result = false;
            res.ErrorMsg = string.Join(",", errorList);
        }

        await browser.CloseAsync();

        return res;
    }

    public (bool Result, string? ErrorMsg) FindUnnecessarySymbol(string text)
    {
        var errorList = new List<string>();
        //TODO
        return (true, string.Join(",", errorList));
    }
}

public class TResult
{
    public bool Result { get; set; }
    public string? ErrorMsg { get; set; }

    public TResult(){
        Result = true;
        ErrorMsg = "";
    }
}
