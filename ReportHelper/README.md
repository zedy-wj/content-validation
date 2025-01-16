<!-- # ReportHelper Project

## Overview
The `ReportHelper` project 用于处理和比较数据，生成不同格式的报告

## Code
- appsettings.json： 配置文件，用于指定Program.cs程序运行时需要对比的包的名称
- **Program.cs**:  项目的入口点，读取本次测试数据和前一次的测试数据，调用比较方法，并将结果保存为 JSON ， Excel，TXT 文件。
  
- **ConstData.cs**:定义了数据的读取和存储的文件路径

- **Models.cs**: 定义了测试结果的数据结构，用于将程序中的数据保存为json文件或将json文件的数据读取到程序中
- **ReportHelper4Test**：提供了将测试结果或比较数据保存为 JSON,Excel,txt( Markdown 格式,用于在 GitHub 上提交issue) 文件的方法。 -->

# ReportHelper Project

## Overview
The `ReportHelper` project is used for processing and comparing data, as well as generating reports in various formats.

## Code Components

- **appsettings.json**  
  A configuration file used to specify which package's test data will be compared during the execution of the Program.cs.
- **Program.cs**  
  Acts as the entry point of the project. It reads the current and previous test data, invokes comparison methods, and saves the results in JSON, Excel, and TXT formats.

- **ConstData.cs**  
  Defines file paths for reading and storing data.

- **Models.cs**  
  Defines the data structure for test results. It facilitates saving program data as JSON files and loading JSON file data into the program.

- **ReportHelper4Test.cs**  
  Provides methods to save test results or comparison data in JSON, Excel, and TXT formats (Markdown format, designed for submitting issues on GitHub).