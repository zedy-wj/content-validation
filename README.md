# Azure Data SDK Content Validation Automation

## Prerequisites

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio

## Quickstart

There are three projects in this solution.

- **ContentValidation**: Libraries container all verify rule.
- **ContentValidation.Test**: Contains all test cases for content validation automation.
- **DataSource**: Provide test data for test cases (all pages need to be verified on [Microsoft Learn website](https://learn.microsoft.com/en-us/python/api/overview/azure/?view=azure-python))

This quickstart will show you how to use this tool to fetch all test data and run test cases.

1. Clone this repo and open it with Visual Studio.
2. Update the `DataSource/appsettings.json` file and replace its content with the service and package you need to test. Currently, only one package can be run at a time, and there are strict requirements for the input of ServiceName and PackageName. Please update according to the official SDK name. For example:
```json
{
  "ServiceName": "App Configuration",
  "PackageName": "azure-appconfiguration"
}
```
3. Run `DataSource` with Visual Studio. After that, you will see an `appsettings.json` file generated under `ContentValidation.Test` folder
4. Switch to `ContentValidation.Test` project and execute test cases.
