<!-- 

# ValidationRule.Test library

## Overview
`ValidationRule.Test` 库主要用于测试 `ContentValidation` 库中的每个验证方法是否能够正确地检测到 SDK 文档内容中的特定错误。每次修改`ContentValidation` 库的验证方法时，需要确保可以通过`ValidationRule.Test` 库的测试用例，来确保 `ContentValidation` 库的代码健壮性和准确性。

## Code Components

- TestValidations.cs 
 `````使用 `ValidationFactory` 创建`ContentValidation` 库中的验证实例，并执行验证方法。``````删除了

 通过读取 LocalHtmlData.json 中的配置数据，传入对应的html页面路径，执行相应的验证规则，并将测试结果与预期结果进行比较。


- HTML folder 
存储测试用的 HTML 文件或片段，包含各种可能的错误情况，用于验证不同的验证规则是否能够正确检测到这些错误。

- LocalHtmlData.json 
 测试用例的配置文件。每个测试用例包括要验证的rule， HTML 的路径和预期的验证结果。 -->

 
# ValidationRule.Test Library

## Overview
The `ValidationRule.Test` library is designed to test whether each validation method in the `ContentValidation` library can accurately detect specific errors in SDK documentation content.   
Whenever a validation method in the `ContentValidation` library is modified, it is essential to ensure the tests in the `ValidationRule.Test` library pass, guaranteeing the robustness and accuracy of the `ContentValidation` library.

## Code Components

- **TestValidations.cs**  
   Reads configuration data from `LocalHtmlData.json`, input the corresponding HTML page paths, then execute the relevant validation methods, and compares the test results with expected results.

- **HTML Folder**  
  Stores HTML files or fragments for testing purposes. These files include various potential error scenarios, enabling validation rules to be tested for their ability to detect these issues accurately.

- **LocalHtmlData.json**  
  The configuration file for test cases. Each test case specifies the validation rule, the HTML file path, and the expected validation results.
 
