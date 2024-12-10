using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace UtilityLibraries
{
    public class GarbledTextValidation : IValidation
    {
        private IPlaywright _playwright;

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

            // Get all text content of the current html.
            var htmlText = await page.Locator("html").InnerTextAsync();

            MatchGarbledText(errorList, htmlText);

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

        private void MatchGarbledText(List<string> errorList, string htmlText){
            string pattern = @":[\w]+(?:\s+[\w]+){0,2}:";
            MatchCollection matches = Regex.Matches(htmlText, pattern);

            // Add the results of regular matching to errorList in a loop.
            foreach (Match match in matches)
            {
                if(match.Value == ":mm:" || match.Value == ":05:"){
                    continue;
                }

                errorList.Add(match.Value);
            }
        }

        public bool ValidateRule(string htmlText){
            var errorList = new List<string>();

            MatchGarbledText(errorList, htmlText);

            return errorList.Count > 0 ? false : true;
        }
    }
}