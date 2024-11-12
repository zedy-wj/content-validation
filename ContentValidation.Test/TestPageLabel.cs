using Microsoft.Playwright;
using System.Text.Json;
using UtilityLibraries;

namespace ContentValidation.Test
{
    public class TestPageLabel
    {
        public static List<string> TestLinks { get; set; }
        public static LabelValidation Validation { get; set; }

        static TestPageLabel()
        {
            TestLinks = JsonSerializer.Deserialize<List<string>>(File.ReadAllText("../../../appsettings.json")) ?? new List<string>();
            Validation = new LabelValidation();
        }

        [Test]
        [TestCaseSource(nameof(TestLinks))]
        public async Task TestExtraLabel(string testLink)
        {
            var playwright = await Playwright.CreateAsync();

            Validation = new LabelValidation(playwright);

            var res = await Validation.FindExtraLabel(testLink);

            Assert.That(res.Result, testLink + " has extra label of  " + res.ErrorMsg);

        }
    }
}

