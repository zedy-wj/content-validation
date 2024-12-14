using Microsoft.Playwright;
using UtilityLibraries;

namespace ValidationRule.Test
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class TestValidations
    {
        public static List<LocalDataItem> TestItems { get; set; }

        static TestValidations()
        {
            TestItems = LocalData.Items;
        }

        [Test]
        [TestCaseSource(nameof(TestItems))]
        public async Task TestExtraLabelValidation(LocalDataItem testItem)
        {
            var playwright = await Playwright.CreateAsync();

            IValidation Validation = new ExtraLabelValidation(playwright);

            if (testItem.Type != "ExtraLabelValidation") return;

            foreach (var rule in testItem.Rules)
            {

                var res = await Validation.Validate(rule.FileUri);

                string errorMessage = @$"The unequal number of errors:
                Validation-Type : {testItem.Type} 
                    Validation-Rule : {rule.RuleName}
                    Expected-Count : {rule.Count}
                    But was: {res.NumberOfOccurrences}
                ";
                
                Assert.That(res.NumberOfOccurrences, Is.EqualTo(rule.Count), errorMessage);
            }

            playwright.Dispose();
        }
    }
}

