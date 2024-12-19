using Microsoft.Playwright;
using System.Collections.Concurrent;
using System.Text.Json;
using UtilityLibraries;
using ReportHelper;

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
            // TestLinks = JsonSerializer.Deserialize<List<string>>(File.ReadAllText("../../../appsettings.json")) ?? new List<string>();

            TestLinks = new List<string>{
                    "https://learn.microsoft.com/en-us/python/api/overview/azure/search?view=azure-python",
    "https://learn.microsoft.com/en-us/python/api/overview/azure/search-documents-readme?view=azure-python",
    "https://learn.microsoft.com/en-us/python/api/azure-search-documents/azure.search.documents?view=azure-python",
    "https://learn.microsoft.com/en-us/python/api/azure-search-documents/azure.search.documents.aio?view=azure-python",
    "https://learn.microsoft.com/en-us/python/api/azure-search-documents/azure.search.documents.aio.asyncsearchitempaged?view=azure-python",
    "https://learn.microsoft.com/en-us/python/api/azure-search-documents/azure.search.documents.aio.searchclient?view=azure-python",
    "https://learn.microsoft.com/en-us/python/api/azure-search-documents/azure.search.documents.aio.searchindexingbufferedsender?view=azure-python",
    "https://learn.microsoft.com/en-us/python/api/azure-search-documents/azure.search.documents.indexes?view=azure-python",
    "https://learn.microsoft.com/en-us/python/api/azure-search-documents/azure.search.documents.indexes.aio?view=azure-python",
    "https://learn.microsoft.com/en-us/python/api/azure-search-documents/azure.search.documents.indexes.aio.searchindexclient?view=azure-python",
    "https://learn.microsoft.com/en-us/python/api/azure-search-documents/azure.search.documents.indexes.aio.searchindexerclient?view=azure-python",
            };
        }

        [OneTimeTearDown]
        public void SaveTestData()
        {
            string excelFilePath = ConstData.TotalIssuesExcelFileName;
            string sheetName = "TotalIssues";
            string jsonFilePath = ConstData.TotalIssuesJsonFileName;

            ExcelHelper4Test.AddTestResult(TestExtraLabelResults, excelFilePath, sheetName);
            ExcelHelper4Test.AddTestResult(TestUnnecessarySymbolsResults, excelFilePath, sheetName);
            JsonHelper4Test.AddTestResult(TestExtraLabelResults, jsonFilePath);
            JsonHelper4Test.AddTestResult(TestUnnecessarySymbolsResults, jsonFilePath);
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

