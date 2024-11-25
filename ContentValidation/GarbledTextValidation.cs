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
            //Create a browser instance.
            var browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var page = await browser.NewPageAsync();
            await page.GotoAsync(testLink);
            var res = new TResult();

            // Fetch all <p> tags
            var pLocators = await page.Locator("p").AllAsync();

            // Loop through each <p> tag.
            foreach (var pLocator in pLocators)
            {
                var text = await pLocator.TextContentAsync();

                //Check if the text is garbled, if so, return the garbled text
                if (Regex.IsMatch(text, @":[\w]+(?:\s+[\w]+){0,2}:"))
                {
                    res.Result = false;
                    res.NumberOfOccurrences += 1;
                    res.LocationsOfErrors.Add($"{res.NumberOfOccurrences}. " + text);
                }
            }
            res.ErrorLink = testLink;
            res.ErrorInfo = "The test link has garbled text";

            await browser.CloseAsync();
            return res;
        }
    }
}
