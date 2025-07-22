# Content Validation Library

## Overview
The Content Validation library is the core module of this repository, which contains a wealth of rules for document validation of SDK Microsoft Learn Doc in various (python/java/...) languages.

The automated development of text review is a very challenging task. The difficulty lies in the fact that there will be various types of errors, including but not limited to: formatting problems, garbled characters, missing type annotations (only for Python doc), etc.

Therefore, when designing validation rules (usually using regular matching), we will expand the scope of fuzzy matching, which helps us catch more errors. But the flaws are also obvious, and many normal texts will also be included in the error list when matched. Accordingly, we designed the `ignore.json` file, in which we can put the matching content that needs to be filtered out to prevent "exception match".

## Getting started
The current validation rules can fully cover the content validation of [Python SDK Microsoft Learn website](https://learn.microsoft.com/en-us/python/api/overview/azure/?view=azure-python), and some rules can be reused on [Java SDK Microsoft Learn website](https://learn.microsoft.com/en-us/java/api/overview/azure/?view=azure-java). We will develop more rules in the future to meet the content validation of all language SDKs.

For a detailed introduction to the rules, please refer to the following table. You can view the specific design of the rules in the markdown files of the respective languages.

| Languages | Path | Description | 
| ------- | ---- | ----------- |
| Python | [python-rules.md](../docs/rules-introduction/python-rules.md) | content validation rules designed for [Microsoft Learn website](https://learn.microsoft.com/en-us/python/api/overview/azure/?view=azure-python).|
| Java | [java-rules.md](../docs/rules-introduction/java-rules.md) | content validation rules designed for [Microsoft Learn website](https://learn.microsoft.com/en-us/java/api/overview/azure/?view=azure-java).|
| .NET | [dotnet-rules.md](../docs/rules-introduction/dotnet-rules.md) | content validation rules designed for [Microsoft Learn website](https://learn.microsoft.com/en-us/dotnet/api/overview/azure/?view=azure-dotnet).|
| JavaScript | [javascript-rules.md](../docs/rules-introduction/javascript-rules.md) | content validation rules designed for [Microsoft Learn website](https://learn.microsoft.com/en-us/javascript/api/overview/azure/?view=azure-node-latest).|

## Configuration

You can filter out some unexpected errors by configuring the `ignore.json` file. Compared to hard-coding filter conditions, it is more portable and flexible. Here is an example:

```json
[
    {
        "Rule": "CommonValidation",
        "IgnoreList": [
            {
                "IgnoreText": "from_dict",
                "Usage": "contains",
                "Description": "Example: from_dict(data, key_extractors=None, content_type=None) , Link: https://learn.microsoft.com/en-us/python/api/azure-iot-hub/azure.iot.hub.protocol.models.authentication_mechanism.authenticationmechanism?view=azure-python&branch=main"
            },
            {
                "IgnoreText": "\\[\\s*-?\\d+\\s*,\\s*-?\\d+\\s*\\]",
                "Usage": "regular",
                "Description": "Matches expressions like [1, 2] or [-1, 5]"
            }
        ]
    },
    {
        "Rule": "ExtraLabelValidation",
        "IgnoreList": [
            {
                "IgnoreText": "<img",
                "Usage": "contains",
                "Description": "Example: <img src=\"cid:inline_image\"> , Link: https://learn.microsoft.com/en-us/java/api/overview/azure/communication-email-readme?view=azure-java-preview&branch=main"
            }
        ]
    },
    {
        "Rule": "UnnecessarySymbolsValidation",
        "IgnoreList": [
            {
                "IgnoreText": "str",
                "Usage": "before]",
                "Description": "Example: List[str] - filters 'str' when it appears before closing bracket ] , Link: https://learn.microsoft.com/en-us/python/api/azure-keyvault-keys/azure.keyvault.keys.deletedkey?view=azure-python&branch=main"
            },
            {
                "IgnoreText": "list",
                "Usage": "prefix",
                "Description": "Example: list[RecognizedForm] - filters when text starts with 'list' , Link: https://learn.microsoft.com/en-us/python/api/azure-ai-formrecognizer/azure.ai.formrecognizer.aio.formrecognizerclient?view=azure-python"
            },
            {
                "IgnoreText": " str",
                "Usage": "[contains]",
                "Description": "Example: dict[str, str] - filters ' str' within bracket context , Link: https://learn.microsoft.com/en-us/python/api/azure-storage-blob/azure.storage.blob.aio.containerclient?view=azure-python#azure-storage-blob-aio-containerclient-from-connection-string"
            },
            {
                "IgnoreText": "azure.core.",
                "Usage": "<contains>",
                "Description": "Example: <class 'azure.core.pipeline.policies._universal._Unset'> - filters 'azure.core.' within angle brackets , Link: https://learn.microsoft.com/en-us/python/api/azure-core//azure.core.pipeline.policies.requestidpolicy?view=azure-python&branch=main"
            }
        ]
    },
    {
        "Rule": "GarbledTextValidation",
        "IgnoreList": [
            {
                "IgnoreText": ":mm:",
                "Usage": "contains",
                "Description": "specified in the format 'hh:mm:ss', ':mm:' , Link: https://learn.microsoft.com/en-us/python/api/azure-search-documents/azure.search.documents.indexes.models.indexingparametersconfiguration?view=azure-python"
            }
        ]
    },
    {
        "Rule": "TypeAnnotationValidation",
        "IgnoreList": [
            {
                "IgnoreText": "**kwargs",
                "Usage": "equal",
                "Description": "Example: AuthenticationMechanism(**kwargs) - filters when text exactly equals '**kwargs' , Link: https://learn.microsoft.com/en-us/python/api/azure-iot-hub/azure.iot.hub.protocol.models.authentication_mechanism.authenticationmechanism?view=azure-python&branch=main"
            }
        ]
    },
    {
        "Rule": "MissingContentValidation",
        "IgnoreList": [
            {
                "IgnoreText": "message",
                "Usage": "subsetOfErrorClass",
                "Description": "Example: error.message - filters 'message' as a standard error property , Link: https://learn.microsoft.com/en-us/javascript/api/azure-iot-common/errors.argumenterror?view=azure-node-latest&branch=main#constructors"
            },
            {
                "IgnoreText": "reduce",
                "Usage": "subset",
                "Description": "Example: array.reduce() - filters 'reduce' as a standard array method"
            }
        ]
    }
]
```

>Notes: In the above example, six types of validation rules are introduced to filter out content during content validation:

### Validation Rules
>
>- **CommonValidation** - Filters common programming patterns like standard Python methods (`from_dict`, `serialize`, `deserialize`) and mathematical expressions with regular expressions.
>- **ExtraLabelValidation** - Filters legitimate HTML/XML tags that might be incorrectly flagged as errors.
>- **UnnecessarySymbolsValidation** - Filters bracket usage in type annotations, generic types, and URL patterns that are syntactically correct.
>- **GarbledTextValidation** - Filters time formats, timestamps, and special identifiers that might appear as garbled text but are legitimate.
>- **TypeAnnotationValidation** - Filters Python function signature syntax like `*args`, `**kwargs`, and parameter annotations.
>- **MissingContentValidation** - Filters standard error class properties and array methods that are expected to be present.

### Configuration Structure
>
>- **Rule** - One of the validation rules where your filter condition needs to be added.
>- **IgnoreList** - This is an array of objects in JSON, each object contains:
>   - **IgnoreText** - The content that actually needs to be filtered out.
>   - **Usage** - Defines the filtering logic with examples:
>     - `contains` - Example: "from_dict" filters any text containing "from_dict"
>     - `equal` - Example: "**kwargs" filters text exactly matching "**kwargs"
>     - `prefix` - Example: "list" filters text starting with "list" like "list[str]"
>     - `before]` - Example: "str" filters "str" when it appears before closing bracket "]"
>     - `[contains]` - Example: " str" filters " str" within bracket context like "dict[str, int]"
>     - `[contain]` - Example: "list" filters "list" within brackets with slight variation
>     - `<contains>` - Example: "azure.core." filters "azure.core." within angle brackets like "<azure.core.class>"
>     - `regular` - Example: "\\[\\s*-?\\d+\\s*,\\s*-?\\d+\\s*\\]" uses regex to match "[1, 2]"
>     - `subsetOfErrorClass` - Example: "message" filters "message" as standard error property
>     - `subset` - Example: "reduce" filters "reduce" as standard array method
>   - **Description** - Not used in rule code, but only used as a comment in JSON for
