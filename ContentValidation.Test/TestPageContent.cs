using Microsoft.Playwright;
using System.Collections.Concurrent;
using System.Text.Json;
using UtilityLibraries;
using ReportHelper;

namespace ContentValidation.Test
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class TestPageContent
    {
        public static List<string> TestLinks { get; set; }
        public static List<string> DuplicateTestLink { get; set; }

        public static ConcurrentQueue<TResult> TestTableMissingContentResults = new ConcurrentQueue<TResult>();

        public static ConcurrentQueue<TResult> TestDuplicateServiceResults = new ConcurrentQueue<TResult>();

        public static ConcurrentQueue<TResult> TestInconsistentTextFormatResults = new ConcurrentQueue<TResult>();



        static TestPageContent()
        {
            TestLinks = JsonSerializer.Deserialize<List<string>>(File.ReadAllText("../../../appsettings.json")) ?? new List<string>();

            //This list is for testing duplicate services.
            DuplicateTestLink = new List<string>
            {
                "https://learn.microsoft.com/en-us/java/api/overview/azure/?view=azure-java-stable"
            };
        }


        [OneTimeTearDown]
        public void SaveTestData()
        {
            string excelFilePath = ConstData.TotalIssuesExcelFileName;
            string sheetName = "TotalIssues";
            string jsonFilePath = ConstData.TotalIssuesJsonFileName;
            ExcelHelper4Test.AddTestResult(TestTableMissingContentResults, excelFilePath, sheetName);
            ExcelHelper4Test.AddTestResult(TestDuplicateServiceResults, excelFilePath, sheetName);
            ExcelHelper4Test.AddTestResult(TestInconsistentTextFormatResults, excelFilePath, sheetName);
            JsonHelper4Test.AddTestResult(TestTableMissingContentResults,jsonFilePath);
            JsonHelper4Test.AddTestResult(TestDuplicateServiceResults,jsonFilePath);
            JsonHelper4Test.AddTestResult(TestInconsistentTextFormatResults, jsonFilePath);
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

        }

        [Test]
        [TestCaseSource(nameof(TestLinks))]
        public async Task TestInconsistentTextFormat(string testLink)
        {
            var playwright = await Playwright.CreateAsync();

            IValidation Validation = new InconsistentTextFormatValidation(playwright);

            var res = await Validation.Validate(testLink);

            res.TestCase = "TestInconsistentTextFormat";
            if (!res.Result)
            {
                TestInconsistentTextFormatResults.Enqueue(res);
            }

            playwright.Dispose();

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

