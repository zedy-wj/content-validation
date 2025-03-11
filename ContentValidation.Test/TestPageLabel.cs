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

        public static ConcurrentQueue<TResult> TestInvalidTagsResults = new ConcurrentQueue<TResult>();

        public static IPlaywright playwright;

        static TestPageLabel()
        {
            playwright = Playwright.CreateAsync().GetAwaiter().GetResult();
            TestLinks = JsonSerializer.Deserialize<List<string>>(File.ReadAllText("../../../appsettings.json")) ?? new List<string>();
        }

        [OneTimeTearDown]
        public void SaveTestData()
        {
            playwright?.Dispose();

            string excelFilePath = ConstData.TotalIssuesExcelFileName;
            string sheetName = "TotalIssues";
            string jsonFilePath = ConstData.TotalIssuesJsonFileName;

            ExcelHelper4Test.AddTestResult(TestExtraLabelResults, excelFilePath, sheetName);
            ExcelHelper4Test.AddTestResult(TestUnnecessarySymbolsResults, excelFilePath, sheetName);
            ExcelHelper4Test.AddTestResult(TestInvalidTagsResults, excelFilePath, sheetName);
            JsonHelper4Test.AddTestResult(TestExtraLabelResults, jsonFilePath);
            JsonHelper4Test.AddTestResult(TestUnnecessarySymbolsResults, jsonFilePath);
            JsonHelper4Test.AddTestResult(TestInvalidTagsResults, jsonFilePath);
        }

        [Test]
        [Category("GeneralTest")]
        [TestCaseSource(nameof(TestLinks))]
        public async Task TestExtraLabel(string testLink)
        {

            IValidation Validation = new ExtraLabelValidation(playwright);

            var res = new TResult();
            try
            {
                res = await Validation.Validate(testLink);
                res.TestCase = "TestExtraLabel";
                if (!res.Result)
                {
                    TestExtraLabelResults.Enqueue(res);
                }
            }
            catch
            {
                pipelineStatusHelper.SavePipelineFailedStatus("code error : ExtraLabelValidation");
                throw;
            }

            Assert.That(res.Result, res.FormatErrorMessage());


        }

        [Test]
        [Category("GeneralTest")]
        [TestCaseSource(nameof(TestLinks))]
        public async Task TestUnnecessarySymbols(string testLink)
        {

            var res = new TResult();
            try
            {

                IValidation Validation = new UnnecessarySymbolsValidation(playwright);

                res = await Validation.Validate(testLink);

                res.TestCase = "TestUnnecessarySymbols";
                if (!res.Result)
                {
                    TestUnnecessarySymbolsResults.Enqueue(res);
                }

            }
            catch
            {
                pipelineStatusHelper.SavePipelineFailedStatus("code error : UnnecessarySymbolsValidation");
                throw;
            }

            Assert.That(res.Result, res.FormatErrorMessage());


        }

        [Test]
        [Category("Ignore")]
        [TestCaseSource(nameof(TestLinks))]
        public async Task TestInvalidTags(string testLink)
        {

            var res = new TResult();
            try
            {

                IValidation Validation = new InvalidTagsValidation(playwright);

                res = await Validation.Validate(testLink);

                res.TestCase = "TestInvalidTags";
                if (!res.Result)
                {
                    TestInvalidTagsResults.Enqueue(res);
                }

            }
            catch
            {
                pipelineStatusHelper.SavePipelineFailedStatus("code error : InvalidTagsValidation");
                throw;
            }

            Assert.That(res.Result, res.FormatErrorMessage());


        }
    }
}