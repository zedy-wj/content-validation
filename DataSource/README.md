# Data Source Project

## Overview
The Data Source Project is used to obtain the test data source (all pages need to be verified on [Microsoft Learn website](https://learn.microsoft.com/en-us/python/api/overview/azure/?view=azure-python)).
>Notes: currently only for getting Python SDK doc.

## Getting started
Need to configure `ServiceName` and `PackageName` in the `appsettings.json` file in advance.
```json
{
    "ServiceName": "App Configuration",
    "PackageName": "azure-appconfiguration"
}
```
>Notes: Taking Python as an example, these two parameters need to be filled in strictly according to the format given in the Python SDK doc, otherwise the code will not be able to obtain all the links to be tested for the relevant package.

After configuration, you can run this project directly. After running, you will find that test data has been generated in the [ContentValidation.Test/appsettings.json](../ContentValidation.Test/appsettings.json) file.