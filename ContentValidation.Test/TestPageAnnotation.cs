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

        public static string PackageName { get; set; }

        public static ConcurrentQueue<TResult> TestMissingTypeAnnotationResults = new ConcurrentQueue<TResult>();

        public static IPlaywright playwright;
        static TestPageAnnotation()
        {
            playwright = Playwright.CreateAsync().GetAwaiter().GetResult();
            TestLinks = JsonSerializer.Deserialize<List<string>>(File.ReadAllText("../../../appsettings.json")) ?? new List<string>();
            PackageName = TestLinks.First();
            TestLinks.RemoveAt(0);
        }

        [OneTimeTearDown]
        public void SaveTestData()
        {
            playwright.Dispose();
            string excelFilePath = ConstData.TotalIssuesExcelFileName;
            string sheetName = "TotalIssues";
            string jsonFilePath = ConstData.TotalIssuesJsonFileName;
            ExcelHelper4Test.AddTestResult(TestMissingTypeAnnotationResults, excelFilePath, sheetName);
            JsonHelper4Test.AddTestResult(TestMissingTypeAnnotationResults, jsonFilePath, PackageName);
            CompareData.Main(new string[] {PackageName});
        }

        [Test]
        [Category("PythonTest")]
        [TestCaseSource(nameof(TestLinks))]
        public async Task TestMissingTypeAnnotation(string testLink)
        {

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
        }
    }
}

