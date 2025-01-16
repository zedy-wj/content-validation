<!-- # Content Validation Test Library

## Overview
`The Content Validation Test` 项目通过执行一系列自动化测试用例，完成对SDK文档的内容检查，确保 SDK 文档内容的正确性和一致性。


## 测试用例

每个测试用例代码将读取appsettings.json文件获取要检查的网页link,然后通过调用`Content Validation`项目中提供的对应的`Validation`方法完成验证，将测试结果数据以为 Excel 和 JSON 文件的形式存储在`Reports`文件夹下

- TestPageAnnotation : 
    - TestMissingTypeAnnotation : 检查类和方法参数的类型注释是否缺失。
- TestPageLabel：
    - TestExtraLabel ： 检查页面中是否存在多余的 HTML 标签。
    - TestUnnecessarySymbols : 检查页面内容中是否存在不必要的符号。
- TestPageContent：测试页面内容，检查表格内容是否缺失、是否存在乱码文本以及是否存在重复服务。
    - TestTableMissingContent : 检查表格内容是否缺失。
    - TestGarbledText : 检查页面中是否存在乱码文本。
    - TestDuplicateService : 检查SDK文档首页中是否存在重复的服务。 -->

# ContentValidation.Test Project

## Overview
The `ContentValidation.Test` project performs automated test cases to validate the content of SDK documentation, ensuring its accuracy and consistency.

## Test Cases

Each test case reads the `appsettings.json` file to obtain the webpage links to be checked. Then it invokes the corresponding `Validation` methods provided by the `ContentValidation` library to perform validation. Finally, the test results are saved in the `Reports` folder as Excel and JSON files.

- **TestPageAnnotation**
  - **TestMissingTypeAnnotation**: Checks for missing type annotations for classes and method parameters.
  
- **TestPageLabel**
  - **TestExtraLabel**: Checks for the presence of unnecessary HTML tags on the page.
  - **TestUnnecessarySymbols**: Checks for unnecessary symbols in the page content.

- **TestPageContent** 
  - **TestTableMissingContent**: Checks for missing content in tables.
  - **TestGarbledText**: Checks for garbled text on the page.
  - **TestDuplicateService**: Checks for duplicate services on the homepage of the SDK documentation.