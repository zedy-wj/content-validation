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

            var Validation = new TextValidation(playwright);

            var res = await Validation.FindEmptyTable(testLink);

            Assert.That(res, testLink + " has table is empty.");

        }
    }
}

