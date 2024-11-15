using Microsoft.Playwright;
using System.Text.Json;
using UtilityLibraries;

namespace ContentValidation.Test
{
    public class TestPageContent
    {
        public static List<string> TestLinks { get; set; }

        static TestPageContent()
        {
            //TestLinks = JsonSerializer.Deserialize<List<string>>(File.ReadAllText("../../../appsettings.json")) ?? new List<string>();
            TestLinks = new List<string>()
            {
                "https://learn.microsoft.com/en-us/python/api/overview/azure/alerts-management?view=azure-python",
                "https://learn.microsoft.com/en-us/python/api/overview/azure/app-platform?view=azure-python",
                "https://learn.microsoft.com/en-us/python/api/overview/azure/automanage?view=azure-python"
            };
        }

        [Test]
        [TestCaseSource(nameof(TestLinks))]
        public async Task TestIsTableEmpty(string testLink)
        {
            var playwright = await Playwright.CreateAsync();

            IValidation Validation = new MissingContentValidation(playwright);

            var res = await Validation.Validate(testLink);

            Assert.That(res.Result, testLink + " has table is empty.");

        }
        
        [Test]
        [TestCaseSource(nameof(TestLinks))]
        public async Task TestDuplicateService(string testLink)
        {
            var playwright = await Playwright.CreateAsync();

            IValidation Validation = new DuplicateServiceValidation(playwright);

            var res = await Validation.Validate(testLink);

            Assert.That(res.Result, res.ErrorMsg);

        }
    }
}

