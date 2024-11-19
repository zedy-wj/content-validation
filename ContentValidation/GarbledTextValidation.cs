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
            var browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var page = await browser.NewPageAsync();
            await page.GotoAsync(testLink);
            var res = new TResult();

            // Fetch all <p> tags and <div> tags containing data-heading-level
            var pLocators = await page.Locator("p").AllAsync();
            var headingDivs = await page.Locator("div.heading-wrapper[data-heading-level]").AllAsync();
            var errorMessages = new List<string>(); // Used to store all error messages.

            // Loop through each <p> tag.
            foreach (var pLocator in pLocators)
            {
                var text = await pLocator.TextContentAsync();
                string specificAnchorHref = string.Empty;

                // Fetch the bounding box of the current <p> tag.
                var pBoundingBox = await pLocator.BoundingBoxAsync();

                // Check if the bounding box is valid.
                if (pBoundingBox != null && pBoundingBox.Width > 0 && pBoundingBox.Height > 0)
                {
                    // Calculate the bottom position of the <p> tag.
                    double pBottom = pBoundingBox.Y + pBoundingBox.Height;

                    // Iterate through headingDivs and find the nearest <div> tag.
                    foreach (var divLocator in headingDivs)
                    {
                        // Fetch the bounding box of the current <div> tag.
                        var divBoundingBox = await divLocator.BoundingBoxAsync();

                        // Checks if the bounding box of the current <div> tag is valid and precedes the current <p> tag.
                        if (divBoundingBox != null && divBoundingBox.Y + divBoundingBox.Height < pBottom)
                        {
                            // Extract the link from the nearest <div> tag.
                            var anchorLocators = await divLocator.Locator("a").AllAsync();
                            if (anchorLocators.Count > 0)
                            {
                                specificAnchorHref = await anchorLocators[^1].GetAttributeAsync("href");
                            }
                        }
                    }
                }

                if (Regex.IsMatch(text, @":[\w]+(?:\s+[\w]+){0,2}:"))
                {
                    res.Result = false;
                    var errorMessage = $" \n{text} " + $"\nLink : {testLink}" + $"{specificAnchorHref}\n";
                    errorMessages.Add(errorMessage);
                }
            }
            res.ErrorMsg = string.Join("",errorMessages);

            await browser.CloseAsync();
            return res;
        }
    }
}
