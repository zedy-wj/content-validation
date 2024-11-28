using Microsoft.Playwright;
using System.Text.Json;
using UtilityLibraries;

namespace ContentValidation.Test
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class TestPageAnnotation
    {
        public static List<string> TestLinks { get; set; }

        static TestPageAnnotation()
        {
            TestLinks = JsonSerializer.Deserialize<List<string>>(File.ReadAllText("../../../appsettings.json")) ?? new List<string>();
        }

        [Test]
        [TestCaseSource(nameof(TestLinks))]
        public async Task TestMissingTypeAnnotation(string testLink)
        {
            var playwright = await Playwright.CreateAsync();

            IValidation Validation = new TypeAnnotationValidation(playwright);

            var res = await Validation.Validate(testLink);

            playwright.Dispose();

            Assert.That(res.Result, res.FormatErrorMessage());

        }
    }
}

