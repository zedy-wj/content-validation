using Microsoft.Playwright;
using System.Text.Json;
using UtilityLibraries;

namespace ContentValidation.Test
{
    public class TestPageAnnotation
    {
        public static List<string> TestLinks { get; set; }

        static TestPageAnnotation()
        {
            //TestLinks = JsonSerializer.Deserialize<List<string>>(File.ReadAllText("../../../appsettings.json")) ?? new List<string>();
            TestLinks = new List<string> {
                "https://learn.microsoft.com/en-us/python/api/azure-communication-identity/azure.communication.identity.phonenumberproperties?view=azure-python",
                "https://learn.microsoft.com/en-us/python/api/azure-communication-identity/azure.communication.identity.microsoftteamsappproperties?view=azure-python",
                "https://learn.microsoft.com/en-us/python/api/azure-batch/azure.batch.operations.accountoperations?view=azure-python"
            };
        }

        [Test]
        [TestCaseSource(nameof(TestLinks))]
        public async Task TestMissingTypeAnnotation(string testLink)
        {
            var playwright = await Playwright.CreateAsync();

            IValidationNew Validation = new MissingTypeAnnotation(playwright);

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
    }
}

