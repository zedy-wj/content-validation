using Microsoft.Playwright;
using System.Text.Json;
using UtilityLibraries;

namespace ContentValidation.Test
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class TestPageContent
    {
        public static List<string> TestLinks { get; set; }
        public static List<string> DuplicateTestLink { get; set; }

        static TestPageContent()
        {
            TestLinks = JsonSerializer.Deserialize<List<string>>(File.ReadAllText("../../../appsettings.json")) ?? new List<string>();

            //This list is for testing duplicate services.
            DuplicateTestLink = new List<string>
            {
                "https://learn.microsoft.com/en-us/python/api/overview/azure/?view=azure-python"
            };
        }

        [Test]
        [TestCaseSource(nameof(TestLinks))]
        public async Task TestTableMissingContent(string testLink)
        {
            var playwright = await Playwright.CreateAsync();

            IValidation Validation = new MissingContentValidation(playwright);

            var res = await Validation.Validate(testLink);

            playwright.Dispose();

            Assert.That(res.Result, res.FormatErrorMessage());

        }

        [Test]
        [TestCaseSource(nameof(TestLinks))]
        public async Task TestGarbledText(string testLink)
        {
            var playwright = await Playwright.CreateAsync();

            IValidation Validation = new GarbledTextValidation(playwright);

            var res = await Validation.Validate(testLink);

            playwright.Dispose();

            Assert.That(res.Result, res.FormatErrorMessage());

        }

        [Test]
        [Ignore("Waiting discussion")]
        [TestCaseSource(nameof(DuplicateTestLink))]
        public async Task TestDuplicateService(string testLink)
        {
            var playwright = await Playwright.CreateAsync();

            IValidation Validation = new DuplicateServiceValidation(playwright);

            var res = await Validation.Validate(testLink);

            playwright.Dispose();

            Assert.That(res.Result, res.FormatErrorMessage());

        }
    }
}

