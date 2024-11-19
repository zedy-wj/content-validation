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
            TestLinks = JsonSerializer.Deserialize<List<string>>(File.ReadAllText("../../../appsettings.json")) ?? new List<string>();
        }

        [Test]
        [TestCaseSource(nameof(TestLinks))]
        public async Task TestIsTableEmpty(string testLink)
        {
            var playwright = await Playwright.CreateAsync();

            IValidation Validation = new MissingContentValidation(playwright);

            var res = await Validation.Validate(testLink);

            Assert.That(res.Result, testLink + " has table is empty." + res.ErrorMsg);

        }

        [Test]
        [TestCaseSource(nameof(TestLinks))]
        public async Task TestGarbledText(string testLink)
        {
            var playwright = await Playwright.CreateAsync();

            IValidation Validation = new GarbledTextValidation(playwright);

            var res = await Validation.Validate(testLink);

            Assert.That(res.Result, testLink +" have Garbled Text:\n" + res.ErrorMsg);

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

