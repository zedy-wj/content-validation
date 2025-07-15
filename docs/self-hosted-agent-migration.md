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

## 问题 2: Bash 脚本在 Windows Self-Hosted Agent 上的兼容性问题

### 错误现象
```
chmod: cannot access 'C:agent_work1s/eng/scripts/push-pipeline-result-markdown.sh': No such file or directory
##[error]Bash exited with code '1'.
```

### 根本原因
1. **路径转换问题**：Windows 路径 `C:\agent\_work\1\s` 在 WSL/Bash 中转换不正确
2. **命令兼容性**：`chmod` 是 Unix/Linux 命令，在 Windows 环境中不适用
3. **文件系统差异**：Windows 和 Linux 的文件权限模型不同

### 解决方案

#### 1. 创建 PowerShell 版本的脚本

创建 `eng/scripts/push-pipeline-result-markdown.ps1`：

```powershell
param(
    [Parameter(Mandatory=$true)][string]$GitHubPat,
    [Parameter(Mandatory=$true)][string]$RepoOwner,
    [Parameter(Mandatory=$true)][string]$RepoName,
    [Parameter(Mandatory=$true)][string]$GitUserName,
    [Parameter(Mandatory=$true)][string]$GitUserEmail,
    [Parameter(Mandatory=$true)][string]$Language
)

$ErrorActionPreference = "Stop"

try {
    $language = $Language.ToLower()
    $repoUrl = "https://github.com/$RepoOwner/$RepoName.git"
    $cloneDir = "./repo-clone"
    $branch = "latest-pipeline-result"
    $fileName = "latest-pipeline-result-for-$language.md"
    
    # 清理现有目录
    if (Test-Path $cloneDir) {
        Remove-Item -Path $cloneDir -Recurse -Force
    }
    
    # 克隆仓库
    $cloneUrl = "https://$GitHubPat@github.com/$RepoOwner/$RepoName.git"
    git clone $cloneUrl $cloneDir
    
    Set-Location $cloneDir
    
    # 检出分支
    git checkout $branch
    if ($LASTEXITCODE -ne 0) {
        git checkout -b $branch
    }
    git pull origin $branch
    
    # 复制文件
    $sourceFile = "../$fileName"
    if (Test-Path $sourceFile) {
        Copy-Item -Path $sourceFile -Destination . -Force
    } else {
        "# Latest Pipeline Result for $Language" | Out-File -FilePath $fileName -Encoding UTF8
    }
    
    # 配置 Git 并提交
    git config --global user.email $GitUserEmail
    git config --global user.name $GitUserName
    git add $fileName
    git commit -m "Updating the latest pipeline result"
    git push origin $branch
    
} catch {
    Write-Error "Error during git operations: $_"
    exit 1
} finally {
    Set-Location ".."
    if (Test-Path $cloneDir) {
        Remove-Item -Path $cloneDir -Recurse -Force -ErrorAction SilentlyContinue
    }
}
```

#### 2. 更新 Pipeline 配置

将原有的 Bash 任务替换为跨平台兼容的 PowerShell 任务：

**修改前：**
```yaml
- task: Bash@3
  displayName: 'Set Script Executable'
  inputs:
    targetType: 'inline'
    script: |
      chmod +x $(System.DefaultWorkingDirectory)/eng/scripts/push-pipeline-result-markdown.sh

- task: Bash@3
  displayName: 'Push commit about current pipeline status'
  inputs:
    targetType: 'filePath'
    filePath: '$(System.DefaultWorkingDirectory)/eng/scripts/push-pipeline-result-markdown.sh'
    arguments: >-
      $(GITHUB_PAT) $(GITHUB_OWNER) $(GITHUB_REPO) 
      $(GIT_USER_NAME) $(GIT_USER_EMAIL) ${{ parameters.language }}
```

**修改后：**
```yaml
- task: PowerShell@2
  displayName: 'Push commit about current pipeline status'
  inputs:
    targetType: 'inline'
    script: |
      $workingDir = "$(System.DefaultWorkingDirectory)"
      $agentOS = "$(Agent.OS)"
      
      if ($agentOS -eq "Windows_NT") {
        # 使用 PowerShell 脚本处理 Windows
        $scriptPath = "$workingDir/eng/scripts/push-pipeline-result-markdown.ps1"
        if (Test-Path $scriptPath) {
          & $scriptPath -GitHubPat "$(GITHUB_PAT)" -RepoOwner "$(GITHUB_OWNER)" -RepoName "$(GITHUB_REPO)" -GitUserName "$(GIT_USER_NAME)" -GitUserEmail "$(GIT_USER_EMAIL)" -Language "${{ parameters.language }}"
        } else {
          Write-Error "PowerShell script not found"
          exit 1
        }
      } else {
        # 使用 Bash 脚本处理 Linux/macOS
        $bashScript = "$workingDir/eng/scripts/push-pipeline-result-markdown.sh"
        if (Test-Path $bashScript) {
          bash -c "chmod +x '$bashScript'"
          bash -c "'$bashScript' '$(GITHUB_PAT)' '$(GITHUB_OWNER)' '$(GITHUB_REPO)' '$(GIT_USER_NAME)' '$(GIT_USER_EMAIL)' '${{ parameters.language }}'"
        } else {
          Write-Error "Bash script not found"
          exit 1
        }
      }
```

### 优势

1. **跨平台兼容**：自动检测操作系统并使用相应的脚本
2. **路径处理正确**：PowerShell 原生处理 Windows 路径
3. **错误处理完善**：提供详细的错误信息和调试输出
4. **维护简单**：保持原有 Bash 脚本的功能不变

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
