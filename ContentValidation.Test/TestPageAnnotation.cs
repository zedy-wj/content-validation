using Microsoft.Playwright;
using System.Text.Json;
using UtilityLibraries;
using System.Collections.Concurrent;

namespace ContentValidation.Test
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class TestPageAnnotation
    {
        public static List<string> TestLinks { get; set; }

        public static ConcurrentQueue<TResult> TestMissingTypeAnnotationResults = new ConcurrentQueue<TResult>();

        static TestPageAnnotation()
        {
            TestLinks = JsonSerializer.Deserialize<List<string>>(File.ReadAllText("../../../appsettings.json")) ?? new List<string>();
        }

        [OneTimeTearDown]
        public void SaveTestData()
        {
            ExcelHelper4Test.AddTestResult(TestMissingTypeAnnotationResults);
            JsonHelper4Test.AddTestResult(TestMissingTypeAnnotationResults);
        }

        [Test]
        [TestCaseSource(nameof(TestLinks))]
        public async Task TestMissingTypeAnnotation(string testLink)
        {
            var playwright = await Playwright.CreateAsync();

            IValidation Validation = new TypeAnnotationValidation(playwright);

            var res = await Validation.Validate(testLink);
            res.TestCase = "TestMissingTypeAnnotation";
            if (!res.Result)
            {
                TestMissingTypeAnnotationResults.Enqueue(res);
            }
            
            playwright.Dispose();

            Assert.That(res.Result, res.FormatErrorMessage());

        }
    }
}

