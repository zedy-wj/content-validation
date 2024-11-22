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
                "https://learn.microsoft.com/en-us/python/api/azure-communication-identity/azure.communication.identity.unknownidentifier?view=azure-python"
            };
        }

        [Test]
        [TestCaseSource(nameof(TestLinks))]
        public async Task TestMissingTypeAnnotation(string testLink)
        {
            var playwright = await Playwright.CreateAsync();

            IValidationNew Validation = new MissingTypeAnnotation(playwright);

            var res = await Validation.Validate(testLink);

            Assert.That(res.Result, res.FormatErrorMessage(res, testLink));

        }
    }
}

