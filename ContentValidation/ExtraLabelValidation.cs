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
        var errorList = new List<string>();

        // Define a list (labelList) containing various HTML tags and entities.
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

        // Create a browser instance.
        var browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync();
        await page.GotoAsync(testLink);

        var text = await page.Locator("html").InnerTextAsync();


        // Iterate through labelList and check if the page content contains any of the tags. If any tags are found, add them to the errorList.
        int sum = 0;
        string errorInfo = "Extra label found: ";
        foreach (var label in labelList)
        {

            int index = 0;
            int count = 0;
            while ((index = text.IndexOf(label, index)) != -1)
            {
                if(text.IndexOf("<true", index) == index){
                    index += 5;
                    continue;
                }
                count++;
                sum++;
                index += label.Length;
            }
            if (count > 0)
            {
                errorInfo += label;
                errorList.Add($"{errorList.Count + 1}. Appears {count} times , label : {label} ");
            }

        }

        if (sum > 0)
        {
            res.Result = false;
            res.ErrorLink = testLink;
            res.ErrorInfo = errorInfo;
            res.NumberOfOccurrences = sum;
            res.LocationsOfErrors = errorList;
        }

        await browser.CloseAsync();

        return res;
    }
}