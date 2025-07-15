# Self-Hosted Agent 迁移指南

## 问题描述

从 Microsoft-hosted agent 迁移到 self-hosted agent 时，Azure DevOps 预定义变量的行为可能会有所不同，特别是：

- `$(System.CollectionUri)`
- `$(System.TeamProject)` 
- `$(Build.BuildId)`
- `$(System.DefaultWorkingDirectory)`

## 解决方案

### 1. 使用 PowerShell 任务替代 Script 任务

**原因：** PowerShell 任务在 self-hosted agent 上对变量解析更可靠。

**修改前：**
```yaml
- script: |
    organization=$(System.CollectionUri)
    project=$(System.TeamProject)
    buildId=$(Build.BuildId)
    pipelineUrl="${organization}${project}/_build/results?buildId=${buildId}"
    echo "##vso[task.setvariable variable=pipelineUrl;]$pipelineUrl"
  displayName: Get the current pipeline url
```

**修改后：**
```yaml
- task: PowerShell@2
  displayName: Get the current pipeline url
  inputs:
    targetType: 'inline'
    script: |
      $organization = "$(System.CollectionUri)"
      $project = "$(System.TeamProject)"
      $buildId = "$(Build.BuildId)"
      
      if (-not $organization.EndsWith('/')) {
        $organization += '/'
      }
      
      $pipelineUrl = "${organization}${project}/_build/results?buildId=${buildId}"
      
      Write-Host "Organization: $organization"
      Write-Host "Project: $project"
      Write-Host "Build ID: $buildId"
      Write-Host "Pipeline URL: $pipelineUrl"
      
      Write-Host "##vso[task.setvariable variable=pipelineUrl;]$pipelineUrl"
```

### 2. 添加环境验证步骤

```yaml
- task: PowerShell@2
  displayName: Validate Environment Variables
  inputs:
    targetType: 'inline'
    script: |
      $workingDir = "$(System.DefaultWorkingDirectory)"
      $organization = "$(System.CollectionUri)"
      $project = "$(System.TeamProject)"
      $buildId = "$(Build.BuildId)"
      
      Write-Host "=== Environment Validation ==="
      Write-Host "Working Directory: $workingDir"
      Write-Host "Organization: $organization"
      Write-Host "Project: $project"
      Write-Host "Build ID: $buildId"
      
      if ([string]::IsNullOrEmpty($organization) -or [string]::IsNullOrEmpty($project) -or [string]::IsNullOrEmpty($buildId)) {
        Write-Error "❌ One or more required Azure DevOps variables are missing!"
        exit 1
      } else {
        Write-Host "✓ All required variables are available"
      }
```

### 3. Self-Hosted Agent 配置检查

确保您的 self-hosted agent 配置正确：

1. **Agent 版本：** 确保使用最新版本的 Azure Pipelines Agent
2. **权限：** 确保 agent 服务账户有足够的权限
3. **网络：** 确保 agent 可以访问 Azure DevOps 服务
4. **环境变量：** 检查 agent 是否正确设置了环境变量

### 4. 调试步骤

如果问题仍然存在，添加以下调试步骤：

```yaml
- task: PowerShell@2
  displayName: Debug Environment
  inputs:
    targetType: 'inline'
    script: |
      Write-Host "=== All Environment Variables ==="
      Get-ChildItem env: | Where-Object { $_.Name -like "*BUILD*" -or $_.Name -like "*SYSTEM*" -or $_.Name -like "*AGENT*" } | Sort-Object Name | ForEach-Object {
        Write-Host "$($_.Name) = $($_.Value)"
      }
      
      Write-Host "`n=== Azure DevOps Predefined Variables ==="
      Write-Host "System.CollectionUri: $(System.CollectionUri)"
      Write-Host "System.TeamProject: $(System.TeamProject)"
      Write-Host "Build.BuildId: $(Build.BuildId)"
      Write-Host "System.DefaultWorkingDirectory: $(System.DefaultWorkingDirectory)"
      Write-Host "Agent.Name: $(Agent.Name)"
      Write-Host "Agent.OS: $(Agent.OS)"
```

## 已修改的文件

以下文件已被更新以支持 self-hosted agent：

- `eng/pipelines/javascript/start.yml`
- `eng/pipelines/python/start.yml`
- `eng/pipelines/java/start.yml`

## 其他注意事项

1. **路径分隔符：** 在 Windows self-hosted agent 上，确保路径使用正确的分隔符
2. **工具可用性：** 确保所需的工具（如 .NET SDK）在 agent 机器上可用
3. **防火墙设置：** 确保 agent 可以访问必要的外部资源

## 验证修复

运行 pipeline 后，检查以下输出：

1. 环境验证步骤应该显示所有变量都可用
2. Pipeline URL 应该正确构建
3. 所有后续步骤应该能够访问设置的变量
