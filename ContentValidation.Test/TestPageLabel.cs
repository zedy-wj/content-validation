using Microsoft.Playwright;
using System.Collections.Concurrent;
using System.Text.Json;
using UtilityLibraries;

namespace ContentValidation.Test
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class TestPageLabel
    {
        public static List<string> TestLinks { get; set; }

        public static ConcurrentQueue<TResult> TestExtraLabelResults = new ConcurrentQueue<TResult>();

        public static ConcurrentQueue<TResult> TestUnnecessarySymbolsResults = new ConcurrentQueue<TResult>();

        static TestPageLabel()
        {
            TestLinks = JsonSerializer.Deserialize<List<string>>(File.ReadAllText("../../../appsettings.json")) ?? new List<string>();
        }

        [OneTimeTearDown]
        public void SaveTestData()
        {
            ExcelHelper4Test.AddTestResult(TestExtraLabelResults);
            ExcelHelper4Test.AddTestResult(TestUnnecessarySymbolsResults);
            JsonHelper4Test.AddTestResult(TestExtraLabelResults);
            JsonHelper4Test.AddTestResult(TestUnnecessarySymbolsResults);
        }

        [Test]
        [TestCaseSource(nameof(TestLinks))]
        public async Task TestExtraLabel(string testLink)
        {
            var playwright = await Playwright.CreateAsync();

            IValidation Validation = new ExtraLabelValidation(playwright);

            var res = await Validation.Validate(testLink);

            res.TestCase = "TestExtraLabel";
            if (!res.Result)
            {
                TestExtraLabelResults.Enqueue(res);
            }

            playwright.Dispose();

            Assert.That(res.Result, res.FormatErrorMessage());
        }

        [Test]
        [TestCaseSource(nameof(TestLinks))]
        public async Task TestUnnecessarySymbols(string testLink)
        {
            var playwright = await Playwright.CreateAsync();

            IValidation Validation = new UnnecessarySymbolsValidation(playwright);

            var res = await Validation.Validate(testLink);

            res.TestCase = "TestUnnecessarySymbols";
            if (!res.Result)
            {
                TestUnnecessarySymbolsResults.Enqueue(res);
            }

            playwright.Dispose();

            Assert.That(res.Result, res.FormatErrorMessage());
        }
    }
}

