using Microsoft.Playwright;
using System.Text.Json;
using UtilityLibraries;

namespace ContentValidation.Test
{
    public class TestPageLabel
    {
        public static List<string> TestLinks { get; set; }

        static TestPageLabel()
        {
            TestLinks = JsonSerializer.Deserialize<List<string>>(File.ReadAllText("../../../appsettings.json")) ?? new List<string>();
        }

        [Test]
        [TestCaseSource(nameof(TestLinks))]
        public async Task TestExtraLabel(string testLink)
        {
            var playwright = await Playwright.CreateAsync();

            IValidation Validation = new ExtraLabelValidation(playwright);

            var res = await Validation.Validate(testLink);

            Assert.That(res.Result, testLink + " has extra label \n\n" + res.Display());
        }

        [Test]
        [TestCaseSource(nameof(TestLinks))]
        public async Task TestUnnecessarySymbols(string testLink)
        {
            var playwright = await Playwright.CreateAsync();

            IValidationNew Validation = new UnnecessarySymbolsValidation(playwright);

            var res = await Validation.Validate(testLink);

            Assert.That(res.Result, res.FormatErrorMessage());
        }
    }
}

