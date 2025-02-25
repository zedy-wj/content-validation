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

        public static ConcurrentQueue<TResult> TestGarbledTextResults = new ConcurrentQueue<TResult>();

        public static ConcurrentQueue<TResult> TestDuplicateServiceResults = new ConcurrentQueue<TResult>();

        public static ConcurrentQueue<TResult> TestInconsistentTextFormatResults = new ConcurrentQueue<TResult>();

        public static IPlaywright playwright;

        static TestPageContent()
        {
            playwright = Playwright.CreateAsync().GetAwaiter().GetResult();
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
            playwright?.Dispose();
            string excelFilePath = ConstData.TotalIssuesExcelFileName;
            string sheetName = "TotalIssues";
            string jsonFilePath = ConstData.TotalIssuesJsonFileName;
            ExcelHelper4Test.AddTestResult(TestTableMissingContentResults, excelFilePath, sheetName);
            ExcelHelper4Test.AddTestResult(TestGarbledTextResults, excelFilePath, sheetName);
            ExcelHelper4Test.AddTestResult(TestDuplicateServiceResults, excelFilePath, sheetName);
            ExcelHelper4Test.AddTestResult(TestInconsistentTextFormatResults, excelFilePath, sheetName);
            JsonHelper4Test.AddTestResult(TestTableMissingContentResults, jsonFilePath);
            JsonHelper4Test.AddTestResult(TestGarbledTextResults, jsonFilePath);
            JsonHelper4Test.AddTestResult(TestDuplicateServiceResults, jsonFilePath);
            JsonHelper4Test.AddTestResult(TestInconsistentTextFormatResults, jsonFilePath);
        }


        [Test]
        [Category("GeneralTest")]
        [TestCaseSource(nameof(TestLinks))]
        public async Task TestTableMissingContent(string testLink)
        {


            IValidation Validation = new MissingContentValidation(playwright);

            var res = new TResult();
            try
            {
                res = await Validation.Validate(testLink);
                res.TestCase = "TestTableMissingContent";
                if (!res.Result)
                {
                    TestTableMissingContentResults.Enqueue(res);
                }
            }
            catch
            {
                pipelineStatusHelper.SavePipelineFailedStatus("code error : MissingContentValidation");
                throw;
            }

            Assert.That(res.Result, res.FormatErrorMessage());


        }

        [Test]
        [Category("GeneralTest")]
        [TestCaseSource(nameof(TestLinks))]
        public async Task TestGarbledText(string testLink)
        {

            IValidation Validation = new GarbledTextValidation(playwright);

            var res = new TResult();
            try
            {
                res = await Validation.Validate(testLink);
                res.TestCase = "TestGarbledText";
                if (!res.Result)
                {
                    TestGarbledTextResults.Enqueue(res);
                }
            }
            catch
            {
                pipelineStatusHelper.SavePipelineFailedStatus("code error : GarbledTextValidation");
                throw;
            }

            Assert.That(res.Result, res.FormatErrorMessage());


        }


        [Test]
        [Category("GeneralTest")]
        [TestCaseSource(nameof(TestLinks))]
        public async Task TestInconsistentTextFormat(string testLink)
        {

            IValidation Validation = new InconsistentTextFormatValidation(playwright);

            var res = new TResult();
            try
            {
                res = await Validation.Validate(testLink);
                res.TestCase = "TestInconsistentTextFormat";
                if (!res.Result)
                {
                    TestInconsistentTextFormatResults.Enqueue(res);
                }
            }
            catch
            {
                pipelineStatusHelper.SavePipelineFailedStatus("code error : InconsistentTextFormatValidation");
                throw;
            }

            Assert.That(res.Result, res.FormatErrorMessage());


        }

        [Test]
        [Category("SpecialTest")]
        [TestCaseSource(nameof(DuplicateTestLink))]
        public async Task TestDuplicateService(string testLink)
        {


            IValidation Validation = new DuplicateServiceValidation(playwright);

            var res = new TResult();
            try
            {
                res = await Validation.Validate(testLink);
                res.TestCase = "TestDuplicateService";
                if (!res.Result)
                {
                    TestDuplicateServiceResults.Enqueue(res);
                }
            }
            catch
            {
                pipelineStatusHelper.SavePipelineFailedStatus("code error : DuplicateServiceValidation");
                throw;
            }

            Assert.That(res.Result, res.FormatErrorMessage());


        }
    }
}