using Microsoft.Playwright;
using UtilityLibraries;

namespace ValidationRule.Test;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class TestValidations
{
    public static List<LocalHTMLDataItem> TestItems { get; set; }

    static TestValidations()
    {
        TestItems = LocalData.Items;
    }



    [Test]
    [TestCaseSource(nameof(TestItems))]
    public async Task TestAllValidations(LocalHTMLDataItem testItem)
    {
        var playwright = await Playwright.CreateAsync();

        IValidation validation = ValidationFactory.CreateValidation(testItem.Type, playwright);

        foreach (var rule in testItem.Rules)
        {

            var res = await validation.Validate(rule.FileUri!);

            // string logMessage = @$"{testItem.Type} - {rule.RuleName} :  {(res.Result == rule.Expected ? "Passed" : "Failed")}";
            // Console.WriteLine(logMessage);

            string errorMessage = @$"
            =====================================
                Validation-Type : {testItem.Type} 
                    Validation-Rule : {rule.RuleName}
                    failed for the file : {(rule.FileUri?.LastIndexOf("HTML") >= 0 ? rule.FileUri.Substring(rule.FileUri.LastIndexOf("HTML")) : rule.FileUri)}
            =====================================
                ";


            Assert.That(res.Result, Is.EqualTo(rule.Expected), errorMessage);

        }

        playwright.Dispose();
    }

}


public static class ValidationFactory
{
    public static IValidation CreateValidation(string validationType, IPlaywright playwright)
    {
        return validationType switch
        {
            "UnnecessarySymbolsValidation" => new UnnecessarySymbolsValidation(playwright),
            "ExtraLabelValidation" => new ExtraLabelValidation(playwright),
            "TypeAnnotationValidation" => new TypeAnnotationValidation(playwright),
            "GarbledTextValidation" => new GarbledTextValidation(playwright),
            "MissingContentValidation" => new MissingContentValidation(playwright),
            "DuplicateServiceValidation" => new DuplicateServiceValidation(playwright),
            _ => throw new ArgumentException($"Unknown validation type: {validationType}")
        };
    }
}
