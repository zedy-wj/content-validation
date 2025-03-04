using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace UtilityLibraries;

public class UnnecessarySymbolsValidation : IValidation
{
    private IPlaywright _playwright;

    public HashSet<string> valueSet = new HashSet<string>();

    public List<string> errorList = new List<string>();

    public TResult res = new TResult();

    // Prefix list for checking if the content before the "[" is in the list.
    public List<IgnoreItem> prefixList = IgnoreData.GetIgnoreList("UnnecessarySymbolsValidation", "prefix");

    // Content list for checking if the content between "[ ]" is in the list.
    public List<IgnoreItem> contentList = IgnoreData.GetIgnoreList("UnnecessarySymbolsValidation", "content");

    public UnnecessarySymbolsValidation(IPlaywright playwright)
    {
        _playwright = playwright ?? throw new ArgumentNullException(nameof(playwright));
    }

    public async Task<TResult> Validate(string testLink)
    {

        //Create a browser instance.
        var browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync();
        await PlaywrightHelper.GotoageWithRetriesAsync(page, testLink);

        // This method needs to be called before "GetHtmlContent()" because "GetHtmlContent()" will delete the code element.
        //Fetch all 'code' content to store in a list. Use regular expressions to find matching unnecessary symbols.
        var codeBlocks = await page.Locator("code").AllInnerTextsAsync();
        ValidateCodeContent(codeBlocks);

        //Fetch all text content to store in a list. Use regular expressions to find matching unnecessary symbols.
        string htmlContent = await GetHtmlContent(page);
        ValidateHtmlContent(htmlContent);


        var formattedList = errorList
            .GroupBy(item => item)
            .Select((group, Index) => $"{Index + 1}. Appears {group.Count()} times ,  {group.Key}")
            .ToList();

        if (errorList.Count > 0)
        {
            res.Result = false;
            res.ErrorLink = testLink;
            res.NumberOfOccurrences = errorList.Count;
            res.ErrorInfo = $"Unnecessary symbols found: {string.Join(" ,", valueSet)}";
            res.LocationsOfErrors = formattedList;
        }

        await browser.CloseAsync();

        return res;
    }

    private void ValidateCodeContent(IReadOnlyList<string> codeBlocks)
    {
        string includePattern = @"~";

        foreach (string codeBlock in codeBlocks)
        {
            string[] lines = codeBlock.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                var matchCollections = Regex.Matches(line, includePattern);
                foreach (Match match in matchCollections)
                {
                    string unnecessarySymbol = $"\"{match.Value}\""; ;
                    valueSet.Add(unnecessarySymbol);
                    errorList.Add($"Unnecessary symbol: {unnecessarySymbol} in code: {line}");
                }
            }
        }
    }

    private void ValidateHtmlContent(string htmlContent)
    {
        // Usage: Find the text that include [ , ], < , >, &, ~, and /// symbols.
        string includePattern = @"[\[\]<>&~]|/{3}";

        // Usage: When the text contains symbols  < or >, exclude cases where they are used in a comparative context (e.g., a > b).
        string excludePattern1 = @"(?<=\w\s)[<>](?=\s\w)";

        // New pattern to match the specified conditions.(e.g., /** hello , **note:** , "word.)
        string newPatternForJava = @"\s\""[a-zA-Z]+\.|^\s*/?\*\*.*$";

        string[] lines = htmlContent.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);

        for (int index = 0; index < lines.Length; index++)
        {
            string line = lines[index];

            var matchCollections = Regex.Matches(line, includePattern);

            foreach (Match match in matchCollections)
            {
                if (match.Value.Equals("<") || match.Value.Equals(">"))
                {
                    if (Regex.IsMatch(line, excludePattern1))
                    {
                        continue;
                    }
                    // Usage: When the text contains <xref, this case will be categorized as an error of ExtraLabelValidation.
                    if (line.Contains("<xref"))
                    {
                        continue;
                    }
                    // Usage: When the text contains symbols => , -< , ->, exclude cases where they are used in a comparative context (e.g., a > b).
                    // Example: HTMLText - A list of stemming rules in the following format: "word => stem", for example: "ran => run".
                    // Link: https://learn.microsoft.com/en-us/python/api/azure-search-documents/azure.search.documents.indexes.models.stemmeroverridetokenfilter?view=azure-python#keyword-only-parameters
                    int i = match.Index - 1;
                    if (i >= 0 && (line[i] == '=' || line[i] == '-'))
                    {
                        continue;
                    }
                }

                if (match.Value.Equals("[") || match.Value.Equals("]"))
                {
                    if (line.Contains("<xref"))
                    {
                        continue;
                    }
                    if (IsBracketCorrect(line, match.Index))
                    {
                        continue;
                    }
                }

                string unnecessarySymbol = $"\"{match.Value}\""; ;
                valueSet.Add(unnecessarySymbol);
                errorList.Add($"Unnecessary symbol: {unnecessarySymbol} in text: {line}");
            }

            // Check the new patternForJava
            Match matchData = Regex.Match(line, newPatternForJava);
            if (matchData.Success)
            {
                string matchedContent = matchData.Value;
                string unnecessarySymbol = $"\"{matchedContent}\"";
                valueSet.Add(unnecessarySymbol);
                errorList.Add($"Unnecessary symbol: {unnecessarySymbol} in text: {line}");      
            }
        }
    }

    private bool IsBracketCorrect(string input, int index)
    {
        if (input[index] == '[')
        {
            // Extract content between "[" and "]"
            int startIndex = index + 1;
            int endIndex = input.IndexOf("]", startIndex);
            if (endIndex == -1)
            {
                // Don't have a closing bracket "]"
                return false;
            }

            string contentBetweenBrackets = input.Substring(startIndex, endIndex - startIndex);

            if (contentList.Any(content => contentBetweenBrackets.Contains(content.IgnoreText, StringComparison.OrdinalIgnoreCase)))
            {
                // Content between brackets is in the `contentList` list
                return true;
            }

            // Check if the content before "[" is in the prefix list
            foreach (var ignoreItem in prefixList)
            {
                string prefix = ignoreItem.IgnoreText;
                try
                {
                    string prefixStr = input.Substring(startIndex - prefix.Length - 1, prefix.Length);
                    if (prefixStr.Equals(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                catch { }
            }

            // Check if the content between brackets contains spaces
            if (Regex.IsMatch(contentBetweenBrackets, @"^[A-Za-z]+$"))
            {
                return true;
            }
        }

        if (input[index] == ']')
        {
            // Check if ] is closed
            int count = 0;
            for (int i = 0; i < index; i++)
            {
                if (input[i] == '[')
                {
                    count++;
                }
                if (input[i] == ']')
                {
                    count--;
                }
            }
            if (count >= 0)
            {
                return true;
            }

        }
        return false;
    }

    private async Task<string> GetHtmlContent(IPage page)
    {
        var contentElement = page.Locator(".content");

        await contentElement.EvaluateAsync(@"(element) => {
            const codeElements = element.querySelectorAll('code');
            codeElements.forEach(code => code.remove());
        }");

        var text = await contentElement.InnerTextAsync();
        return text;
    }
}