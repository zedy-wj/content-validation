using Microsoft.Playwright;
using System.Text.Json;
using UtilityLibraries;

namespace ContentValidation.Test
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class TestPageLabel
    {
        public static List<string> TestLinks { get; set; }

        static TestPageLabel()
        {
            //TestLinks = JsonSerializer.Deserialize<List<string>>(File.ReadAllText("../../../appsettings.json")) ?? new List<string>();
            TestLinks = new List<string>
            {
                "https://learn.microsoft.com/en-us/python/api/azure-search-documents/azure.search.documents.indexes.searchindexerclient?view=azure-python",
                "https://learn.microsoft.com/en-us/python/api/azure-search-documents/azure.search.documents.searchclient?view=azure-python",
                "https://learn.microsoft.com/en-us/python/api/azure-search-documents/azure.search.documents.searchindexingbufferedsender?view=azure-python",
                "https://learn.microsoft.com/en-us/python/api/azure-search-documents/azure.search.documents.aio.searchclient?view=azure-python",
                "https://learn.microsoft.com/en-us/python/api/azure-search-documents/azure.search.documents.aio.searchindexingbufferedsender?view=azure-python",
                "https://learn.microsoft.com/en-us/python/api/azure-search-documents/azure.search.documents.indexes.aio.searchindexerclient?view=azure-python",
                "https://learn.microsoft.com/en-us/python/api/azure-functions-durable/azure.durable_functions.tasks?view=azure-python#azure-durable-functions-tasks-call-sub-orchestrator-task"
            };
        }

        [Test]
        [TestCaseSource(nameof(TestLinks))]
        public async Task TestExtraLabel(string testLink)
        {
            var playwright = await Playwright.CreateAsync();

            IValidation Validation = new ExtraLabelValidation(playwright);

            var res = await Validation.Validate(testLink);

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

            playwright.Dispose();

            Assert.That(res.Result, res.FormatErrorMessage());
        }
    }
}

