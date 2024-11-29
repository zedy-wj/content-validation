using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace UtilityLibraries
{
    public class GarbledTextValidation : IValidation
    {
        private readonly IPlaywright _playwright;

        public GarbledTextValidation(IPlaywright playwright)
        {
            _playwright = playwright;
        }

        public async Task<TResult> Validate(string testLink)
        {
            var res = new TResult();
            var errorList = new List<string>();

            //Create a browser instance.
            var browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var page = await browser.NewPageAsync();
            await page.GotoAsync(testLink);

            var htmlText = await page.Locator("html").InnerTextAsync();

            string pattern = @":[\w]+(?:\s+[\w]+){0,2}:";
            MatchCollection matches = Regex.Matches(htmlText, pattern);

            foreach (Match match in matches)
            {
                errorList.Add(match.Value);
            }

            var formattedList = errorList
                .GroupBy(item => item)
                .Select((group, Index) => $"{Index + 1}. Appears {group.Count()} times , garbled text :   {group.Key}")
                .ToList();

            if (errorList.Count > 0)
            {
                res.Result = false;
                res.ErrorLink = testLink;
                res.NumberOfOccurrences = errorList.Count;
                res.ErrorInfo = "The test link has garbled text";
                res.LocationsOfErrors = formattedList;
            }

            await browser.CloseAsync();
            return res;
        }
    }
}
