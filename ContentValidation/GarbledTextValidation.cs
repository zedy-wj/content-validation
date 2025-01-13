using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace UtilityLibraries
{
    public class GarbledTextValidation : IValidation
    {
        private IPlaywright _playwright;

        public List<IgnoreItem> ignoreList = IgnoreData.GetIgnoreList("GarbledTextValidation", "");

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
            await PlaywrightHelper.GotoageWithRetriesAsync(page, testLink);

            // Get all text content of the current html.
            var htmlText = await page.Locator("html").InnerTextAsync();

            string pattern = @":[\w]+(?:\s+[\w]+){0,2}:";
            MatchCollection matches = Regex.Matches(htmlText, pattern);

            // Add the results of regular matching to errorList in a loop.
            foreach (Match match in matches)
            {
                //Judge if an element is in the ignoreList.
                bool shouldIgnore = ignoreList.Any(item => string.Equals(item.IgnoreText, match.Value, StringComparison.OrdinalIgnoreCase));

                //If it is not an ignore element, it means that it is garbled text.
                if (!shouldIgnore)
                {
                    errorList.Add(match.Value);
                }
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
                res.ErrorInfo = "The test link has garbled text.";
                res.LocationsOfErrors = formattedList;
            }

            await browser.CloseAsync();

            return res;
        }
    }
}