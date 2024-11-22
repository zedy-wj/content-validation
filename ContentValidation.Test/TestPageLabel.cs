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
            //TestLinks = JsonSerializer.Deserialize<List<string>>(File.ReadAllText("../../../appsettings.json")) ?? new List<string>();
            TestLinks = new List<string>
            {
                "https://learn.microsoft.com/en-us/python/api/azure-security-attestation/azure.security.attestation.aio.attestationadministrationclient?view=azure-python"
            };
        }

        [Test]
        [TestCaseSource(nameof(TestLinks))]
        public async Task TestExtraLabel(string testLink)
        {
            var playwright = await Playwright.CreateAsync();

            IValidationNew Validation = new ExtraLabelValidation(playwright);

            var res = await Validation.Validate(testLink);

            string errorMsg = $@"
ErrorLink: {res.ErrorLink}
ErrorInfo: {res.ErrorInfo}
Number of Occurrences: {res.NumberOfOccurrences}
Locations of Errors: 
        {string.Join("\n\t\t", res.LocationsOfErrors)}
";

            Assert.That(res.Result, errorMsg);
        }

        [Test]
        [TestCaseSource(nameof(TestLinks))]
        public async Task TestUnnecessarySymbols(string testLink)
        {
            var playwright = await Playwright.CreateAsync();

            IValidation Validation = new UnnecessarySymbolsValidation(playwright);

            var res = await Validation.Validate(testLink);

            Assert.That(res.Result, testLink + " has unnecessary symbols:\n  " + res.ErrorMsg);

        }
    }
}

