# Tool Rules Introduction For Python

## Overview

This document introduces 6 rules designed for Python Data SDK on [Microsoft Learn website](https://learn.microsoft.com/en-us/python/api/overview/azure/?view=azure-python) to complete automated content validation.

## Validation Rules

- [ExtraLabelValidation](#1-extralabelvalidation)
- [TypeAnnotationValidation](#2-typeannotationvalidation)
- [UnnecessarySymbolsValidation](#3-unnecessarysymbolsvalidation)
- [MissingContentValidation](#4-missingcontentvalidation)
- [GarbledTextValidation](#5-garbledtextvalidation)
- [DuplicateServiceValidation](#6-duplicateservicevalidation)

### 1. ExtraLabelValidation

- **Goal:**
  This rule detects whether there are front-end tags in the page that are not parsed correctly.

- **Extra Labels:**

  - `<br` , `<span` , `<div`, `<table` , `<img` , `<code` , `<xref`...
    > Note : The extra labels currently detected are `<xref` , `<br` , `<code` , `&gt`.

- **Example:**

  - Extra Label: `<br />`
  - Text Content:
    ` Indicates whether OS upgrades should automatically be applied to scale set instances in a rolling fashion when a newer version of the OS image becomes available. <br />``<br /> If this is set to true for Windows based pools, WindowsConfiguration.enableAutomaticUpdates cannot be set to true. `
  - Link:
    https://learn.microsoft.com/en-us/python/api/azure-mgmt-batch/azure.mgmt.batch.models.automaticosupgradepolicy?view=azure-python#keyword-only-parameters
  - Image:  
    &nbsp;<img src="./image/python-sdk/image-ExtraLabelValidation.png" alt="ExtraLabelValidation" style="width:700px;">

- **Code Snippet:**

  ```csharp

    // Define a list (labelList) containing various HTML tags and entities.
    var labelList = new List<string> {
        "<br",
        "<span",
        "<div",
        "<table",
        "<img",
        "<code",
        "<xref",
        ...
    };

    // Iterate through labelList and check if the page content contains any of the tags. If any tags are found, add them to the errorList.
    foreach (var label in labelList)
    {
        int index = 0;
        // Count the number of all errors for the current label.
        int count = 0;
        while ((index = htmlText.IndexOf(label, index)) != -1)
        {
            ...
            count++;
            sum++;
            index += label.Length;
        }

        ...
    }
  ```

### 2. TypeAnnotationValidation

- **Goal:**
  This rule checks each class and method parameter for correct type annotations and record any missing or incorrect ones.

- **Example:**

  - Missing Type Annotations: `value`
  - Text Content:
    `ShareProtocols(value, names=None, *, module=None, qualname=None, type=None, start=1, boundary=None)`
  - Link:
    https://learn.microsoft.com/en-us/python/api/azure-storage-file-share/azure.storage.fileshare.shareprotocols?view=azure-python#constructor

  - Image:  
    &nbsp;<img src="./image/python-sdk/image-TypeAnnotationValidation.png" alt="TypeAnnotationValidation" style="width:700px;">

- **Code Snippet:**
  ```csharp
    // Determine whether the parameter correctly contains type annotation
    bool IsCorrectTypeAnnotation(string text)
    {
        if (equalList.Any(item => text.Equals(item.IgnoreText)))
        {
            return true;
        }
        if (containList.Any(item => text.Contains(item.IgnoreText)))
        {
            return true;
        }
        if (Regex.IsMatch(text, @"^[^=]+=[^=]+$"))  // pattern like a=b
        {
            return true;
        }
        return false;
    }
  ```

### 3. UnnecessarySymbolsValidation

- **Goal:**
  This rule detects whether there are unnecessary symbols in page content.

- **Unnecessary Symbols:**

  - `<` , `>` , `~` , `[` , `]` , `///`.

- **Example:**

  - Unnecessary Symbols: `~`
  - Text Content:
    `Access for the ~azure.storage.blob.BlobServiceClient. Default is False.`
  - Link:
    https://learn.microsoft.com/en-us/python/api/azure-storage-file-share/azure.storage.fileshare.services?view=azure-python#keyword-only-parameters
  - Image:  
    &nbsp;<img src="./image/python-sdk/image-UnnecessarySymbolsValidation.png" alt="UnnecessarySymbolsValidation" style="width:700px;">

- **Code Snippet:**

  ```csharp

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


  ```

### 4. MissingContentValidation

- **Goal:**
  This rule checks if there is the blank table.

- **Example:**

  - Link:
    https://learn.microsoft.com/en-us/python/api/azure-appconfiguration/azure.appconfiguration.aio.azureappconfigurationclient?view=azure-python#parameters
  - Image:  
    &nbsp;<img src="./image/python-sdk/image-MissingContentValidation.png" alt="MissingContentValidation" style="width:700px;">

- **Code Snippet:**

  ```csharp

      public bool isIgnore = false;
      private async Task ProcessCellAsync(
        IElementHandle cell,
        IPage page,
        string testLink,
        List<string> errorList,
        List<IgnoreItem> ignoreList,
        List<IgnoreItem> ignoreListOfErrorClass,
        bool isColspan2 = false
        )
      {
        var rawText = await cell.EvaluateAsync<string>("el => el.textContent");
        var cellText = rawText?.Trim() ?? "";
    
        // Skip ignored text
        if (ignoreList.Any(item => cellText.Equals(item.IgnoreText, StringComparison.OrdinalIgnoreCase)))
        {
            isIgnore = true;
            return;
        }
    
        if (testLink.Contains("javascript", StringComparison.OrdinalIgnoreCase) && testLink.Contains("errors", StringComparison.OrdinalIgnoreCase))
        {
            if (ignoreListOfErrorClass.Any(item => cellText.Equals(item.IgnoreText, StringComparison.OrdinalIgnoreCase)))
            {
                isIgnore = true;
                return;
            }
        }
    
        if (!isColspan2)
        {
            if (!string.IsNullOrEmpty(cellText))
            {
                isIgnore = false;
                return;
            }
            else
            {
                if (isIgnore)
                {
                    isIgnore = false;
                    return; // Skip if the cell is ignored
                }
            }
        }
    
        var anchorLink = await GetAnchorLinkForCellAsync(cell, page, testLink);
    
        if (anchorLink == "This is an ignore cell, please ignore it.")
        {
            return; // Skip if the anchor link is the ignore text
        }
    
        if (!anchorLink.Contains("#packages", StringComparison.OrdinalIgnoreCase) &&
            !anchorLink.Contains("#modules", StringComparison.OrdinalIgnoreCase))
        {
            errorList.Add(anchorLink);
        }
      }
    


  ```

### 5. GarbledTextValidation

- **Goal:**
  This rule checks whether there is garbled text.

- **Garbled Text:**

  - `:xxxx:`
  - `:xxxx xxxx:`
  - `:xxxx xxxx xxxx:`
  - `<components·ikn5y4·schemas·dppworkerrequest·properties·headers·additionalproperties>`
  - `<components�ikn5y4·schemas�dppworkerrequest�properties�headers�additionalproperties>`

- **Example:**

  - Garbled Text: `:class:`
  - Text Content:
    `Close the :class: ~azure.communication.identity.aio.CommunicationIdentityClient session.`
  - Link:
    https://learn.microsoft.com/en-us/python/api/azure-communication-identity/azure.communication.identity.aio.communicationidentityclient?view=azure-python#methods
  - Image:  
    &nbsp;<img src="./image/python-sdk/image-GarbledTextValidation.png" alt="GarbledTextValidation" style="width:700px;">

- **Code Snippet:**

  ```csharp

    // Get all text content of the current html.
    var htmlText = await page.Locator("html").InnerTextAsync();

    // Usage: This regular expression is used to extract the garbled characters in the format of ":ivar:request_id:/:param cert_file:/:param str proxy_addr:" from the text.
    // Example: Initializer for X509 Certificate :param cert_file: The file path to contents of the certificate (or certificate chain)used to authenticate the device.
    // Link: https://learn.microsoft.com/en-us/python/api/azure-iot-device/azure.iot.device?view=azure-python
    string pattern = @":[\w]+(?:\s+[\w]+){0,2}:|Dictionary of <[^>]*·[^>]*>|Dictionary of <[^>]*\uFFFD[^>]*>";
    MatchCollection matches = Regex.Matches(htmlText, pattern);

    // Add the results of regular matching to errorList in a loop.
    foreach (Match match in matches)
    {
        //Judge if an element is in the ignoreList.
        bool shouldIgnore = ignoreList.Any(item => string.Equals(item.IgnoreText, match.Value, StringComparison.OrdinalIgnoreCase));

        //If it is not an ignore element, it means that it is garbled text.
        if (!shouldIgnore)
        {
            errorList.Add(match.Value);
        }
    }


  ```

### 6. DuplicateServiceValidation

- **Goal:**
  This rule checks whether there is duplicate service.

- **Example:**

  - Link:
    https://learn.microsoft.com/en-us/python/api/overview/azure/?view=azure-python
  - Image:  
    &nbsp;<img src="./image/python-sdk/image-DuplicateServiceValidation.png" alt="DuplicateServiceValidation" style="width:700px;">

- **Code Snippet:**

  ```csharp

    //Get all service tags in the test page.
    var aElements = await page.Locator("li.has-three-text-columns-list-items.is-unstyled a[data-linktype='relative-path']").AllAsync();

    //Check if there are duplicate services.
    foreach (var element in aElements)
    {
        var text = await element.InnerTextAsync();

        //Store the names in the `HashSet`.
        //When `HashSet` returns false, duplicate service names are stored in another array.
        if (!set.Add(text))
        {
            errorList.Add(text);

            res.Result = false;
            res.ErrorLink = testLink;
            res.NumberOfOccurrences += 1;
        }

    }


  ```
