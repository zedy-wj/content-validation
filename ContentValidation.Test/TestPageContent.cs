using Microsoft.Playwright;
using System.Collections.Concurrent;
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

        public static ConcurrentQueue<TResult> TestTableMissingContentResults = new ConcurrentQueue<TResult>();

        public static ConcurrentQueue<TResult> TestGarbledTextResults = new ConcurrentQueue<TResult>();

        public static ConcurrentQueue<TResult> TestDuplicateServiceResults = new ConcurrentQueue<TResult>();



        static TestPageContent()
        {
            TestLinks = JsonSerializer.Deserialize<List<string>>(File.ReadAllText("../../../appsettings.json")) ?? new List<string>();

            //This list is for testing duplicate services.
            DuplicateTestLink = new List<string>
            {
                "https://learn.microsoft.com/en-us/python/api/overview/azure/?view=azure-python"
            };
        }


        [OneTimeTearDown]
        public void SaveTestData()
        {
            ExcelHelper4Test.AddTestResult(TestTableMissingContentResults);
            ExcelHelper4Test.AddTestResult(TestGarbledTextResults);
            ExcelHelper4Test.AddTestResult(TestDuplicateServiceResults);
            JsonHelper4Test.AddTestResult(TestTableMissingContentResults);
            JsonHelper4Test.AddTestResult(TestGarbledTextResults);
            JsonHelper4Test.AddTestResult(TestDuplicateServiceResults);
        }


        [Test]
        [TestCaseSource(nameof(TestLinks))]
        public async Task TestTableMissingContent(string testLink)
        {
            var playwright = await Playwright.CreateAsync();

            IValidation Validation = new MissingContentValidation(playwright);

            var res = await Validation.Validate(testLink);

            res.TestCase = "TestTableMissingContent";
            if (!res.Result)
            {
                TestTableMissingContentResults.Enqueue(res);
            }

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

            res.TestCase = "TestGarbledText";
            if (!res.Result)
            {
                TestGarbledTextResults.Enqueue(res);
            }

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

            res.TestCase = "TestDuplicateService";
            if (!res.Result)
            {
                TestDuplicateServiceResults.Enqueue(res);
            }

            playwright.Dispose();

            Assert.That(res.Result, res.FormatErrorMessage());

        }
    }
}

