using Microsoft.Playwright;
using System.Text.Json;
using UtilityLibraries;
using ReportHelper;
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
            string excelFilePath = ConstData.TotalIssuesExcelFileName;
            string sheetName = "TotalIssues";
            string jsonFilePath = ConstData.TotalIssuesJsonFileName;
            ExcelHelper4Test.AddTestResult(TestMissingTypeAnnotationResults, excelFilePath, sheetName);
            JsonHelper4Test.AddTestResult(TestMissingTypeAnnotationResults, jsonFilePath);
        }

        [Test]
        [Category("GeneralTest")]
        [TestCaseSource(nameof(TestLinks))]
        public async Task TestMissingTypeAnnotation(string testLink)
        {
            var playwright = await Playwright.CreateAsync();
            IValidation Validation = new TypeAnnotationValidation(playwright);

            var res = new TResult();
            try
            {
                res = await Validation.Validate(testLink);
                res.TestCase = "TestMissingTypeAnnotation";
                if (!res.Result)
                {
                    TestMissingTypeAnnotationResults.Enqueue(res);
                }
            }
            catch
            {
                pipelineStatusHelper.SavePipelineFailedStatus("code error : TypeAnnotationValidation");
                throw;
            }

            Assert.That(res.Result, res.FormatErrorMessage());
            playwright.Dispose();
        }
    }
}

