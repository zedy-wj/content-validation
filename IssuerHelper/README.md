# Issuer Helper Project

## Overview
The Issuer Helper Project is used to handle issue-related content. Its content mainly consists of two parts: summarizing the test errors of all packages at the time; generating markdown files to record the current run information.

## Getting started
This project is part of an automated process and has a prerequisite that cannot be completed when running locally: download the artifacts uploaded by the currently running pipeline in the Azure DevOps Pipeline.

When running in the pipeline, a `PackageName` need to be passed. When it is "all", the entire array object is read from the `ConfigureAllPackages.json` file. Currently, we have only tested 10 packages. If you need to add more packages later, please add the relevant packages here.

```json
[
    {
        "packages": [
            "azure-ai-formrecognizer",
            "azure-ai-textanalytics", 
            "azure-appconfiguration", 
            "azure-cosmos", 
            "azure-storage-blob", 
            "azure-keyvault-administration", 
            "azure-keyvault-certificates", 
            "azure-keyvault-keys", 
            "azure-keyvault-secrets", 
            "azure-search-documents"
        ]
    }
]
```