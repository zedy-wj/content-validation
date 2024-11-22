using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace UtilityLibraries
{
    public class GarbledTextValidation : IValidationNew
    {
        private readonly IPlaywright _playwright;

        public GarbledTextValidation(IPlaywright playwright)
        {
            _playwright = playwright;
        }

        public async Task<TResultNew> Validate(string testLink)
        {
            //Create a browser instance.
            var browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var page = await browser.NewPageAsync();
            await page.GotoAsync(testLink);
            var res = new TResultNew();

            // Fetch all <p> tags
            var pLocators = await page.Locator("p").AllAsync();
            var errorMessage = "";

            // Loop through each <p> tag.
            foreach (var pLocator in pLocators)
            {
                var text = await pLocator.TextContentAsync();

                //Check if the text is garbled, if so, return the garbled text
                if (Regex.IsMatch(text, @":[\w]+(?:\s+[\w]+){0,2}:"))
                {
                    res.Result = false;
                    res.NumberOfOccurrences += 1;
                    errorMessage += $"\n{res.NumberOfOccurrences}.\n" + text;
                }
            }

            res.ErrorLink = $"\nError Link: {testLink}\n";
            res.ErrorInfo = "Error Info: The test link has garbled text.\n";
            res.LocationsOfErrors.Add("Locations of Errors:" + errorMessage);

            await browser.CloseAsync();
            return res;
        }
    }
}
