using Microsoft.Playwright;
using UtilityLibraries;

namespace ValidationRule.Test
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]

    public class TestAllValidationRules
    {
        private static string testGarbledTextData { get; set; }

        static TestAllValidationRules()
        {
            testGarbledTextData = File.ReadAllText("../../../StaticData/GarbledTextStaticData.txt");
        }

        [Test]
        public async Task TestGarbledTextValidationRule()
        {
            var playwright = await Playwright.CreateAsync();

            GarbledTextValidation Validation = new GarbledTextValidation(playwright);

            var res = Validation.ValidateRule(testGarbledTextData);
            
            playwright.Dispose();
            
            Assert.IsFalse(res);
        }
    }
}