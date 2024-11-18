using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace UtilityLibraries;

public class MissingTypeAnnotation : IValidation
{
    private IPlaywright _playwright;


    public MissingTypeAnnotation(IPlaywright playwright)
    {
        _playwright = playwright;
    }
    public async Task<TResult> Validate(string testLink)
    {
        var res = new TResult();

        var browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync();
        await page.GotoAsync(testLink);

        Dictionary<string, List<string>>? pyClassParamMap = null;
        Dictionary<string, List<string>>? pyMethodParamMap = null;

        if (testLink.Contains("azuresdkdocs", StringComparison.OrdinalIgnoreCase))
        {
            pyClassParamMap = await GetParamMap4AzureSdkDocs(page, true);
            pyMethodParamMap = await GetParamMap4AzureSdkDocs(page, false);
        }
        else if (testLink.Contains("learn.microsoft", StringComparison.OrdinalIgnoreCase))
        {
            pyClassParamMap = await GetParamMap4LearnMicrosoft(page, true);
            pyMethodParamMap = await GetParamMap4LearnMicrosoft(page, false);
        }

        ValidParamMap(pyClassParamMap!, true, res);
        ValidParamMap(pyMethodParamMap!, false, res);

        if (!res.IsEmpty())
        {
            res.Result = false;
        }

        await browser.CloseAsync();

        return res;
    }

    bool IsCorrectTypeAnnotation(string text)
    {
        if (text == "*")
        {
            return true;
        }
        else if (text == "**kwargs" || text == "*args")
        {
            return false;
        }
        else if (Regex.IsMatch(text, @"^[^=]+=[^=]+$"))
        {
            return true;
        }
        else if (text.Contains(":"))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    async Task<Dictionary<string, List<string>>> GetParamMap4AzureSdkDocs(IPage page, bool isClass)
    {
        Dictionary<string, List<string>> paramMap = new Dictionary<string, List<string>>();

        IReadOnlyList<ILocator>? HTMLElementList = null;
        if (isClass)
        {
            HTMLElementList = await page.Locator(".py.class").AllAsync();
        }
        else
        {
            HTMLElementList = await page.Locator(".py.method").AllAsync();
        }


        for (int i = 0; i < HTMLElementList.Count; i++)
        {
            var HTMLElement = HTMLElementList[i];
            var dtElement = HTMLElement.Locator("dt").Nth(0);
            var key = await dtElement.GetAttributeAsync("id");
            var paramLocatorsList = await dtElement.Locator(".sig-param").AllAsync();

            List<string> paramList = new List<string>();

            foreach (var locator in paramLocatorsList)
            {
                var innerText = await locator.InnerTextAsync();
                paramList.Add(innerText);
            }

            paramMap[key] = paramList;
        }

        return paramMap;
    }

    async Task<Dictionary<string, List<string>>> GetParamMap4LearnMicrosoft(IPage page, bool isClass)
    {
        Dictionary<string, List<string>> paramMap = new Dictionary<string, List<string>>();

        IReadOnlyList<ILocator>? HTMLElementList = null;
        if (isClass)
        {
            HTMLElementList = await page.Locator(".content > .wrap.has-inner-focus").AllAsync();
        }
        else
        {
            HTMLElementList = await page.Locator(".memberInfo > .wrap.has-inner-focus").AllAsync();
        }

        for (int i = 0; i < HTMLElementList.Count; i++)
        {

            var HTMLElement = HTMLElementList[i];
            var codeText = await HTMLElement.InnerTextAsync();

            var regex = new Regex(@"(?<key>.+?)\((?<params>.+?)\)");
            var match = regex.Match(codeText);

            string key = "";
            string paramsText = "";

            if (!match.Success && !isClass)
            {
                Console.WriteLine("Ignore codeText : ");
                Console.WriteLine(codeText);
                Console.WriteLine("");
                continue;
            }

            if (!match.Success && isClass)
            {
                key = codeText;
            }

            if (match.Success)
            {
                key = match.Groups["key"].Value.Trim();
                paramsText = match.Groups["params"].Value.Trim();
            }

            var paramList = SplitParameters(paramsText);

            paramMap[key] = paramList;
        }

        return paramMap;
    }



    List<string> SplitParameters(string paramsText)
    {
        var paramList = new List<string>();
        int bracketCount = 0;
        string currentParam = "";

        for (int i = 0; i < paramsText.Length; i++)
        {
            char c = paramsText[i];

            if (c == '[')
            {
                bracketCount++;
            }
            else if (c == ']')
            {
                bracketCount--;
            }
            else if (c == ',' && bracketCount == 0)
            {
                paramList.Add(currentParam.Trim());
                currentParam = "";
                continue;
            }

            currentParam += c;
        }

        if (!string.IsNullOrWhiteSpace(currentParam))
        {
            paramList.Add(currentParam.Trim());
        }

        return paramList;
    }


    void ValidParamMap(Dictionary<string, List<string>> paramMap, bool isClass, TResult res)
    {

        foreach (var item in paramMap)
        {
            string key = item.Key;
            var paramList = item.Value;

            if (isClass && paramList.Count == 0)
            {
                res.Add($"{key}", "argument", "Class empty argument");
            }

            for (int i = 0; i < paramList.Count; i++)
            {
                var text = paramList[i];

                if (!IsCorrectTypeAnnotation(text))
                {
                    res.Add($"{key}", "argument", text);
                }
            }
        }

    }

}

