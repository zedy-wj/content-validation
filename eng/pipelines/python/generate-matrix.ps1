# 该脚本用于根据 package-readme-mapping.yml 和 pipeline 传入的 packages 参数，生成 matrix yaml 片段
param(
    [string]$Packages,
    [string]$MappingFile = "./eng/pipelines/python/package-readme-mapping.yml",
    [string]$OutputFile = "./eng/pipelines/python/generated-matrix.yml"
)

# 读取映射表
yaml = Get-Content $MappingFile -Raw | ConvertFrom-Yaml

# 处理包名
$packageList = $Packages -split ',' | ForEach-Object { $_.Trim() }

$matrix = @{}
foreach ($pkg in $packageList) {
    $key = $pkg.Trim()
    $readme = $yaml.$key
    if (-not $readme) {
        Write-Host "[Warning] $key not found in mapping, use default rule."
        $readme = "$($key -replace '^azure-','')-readme"
    }
    $matrix[$key.Replace('-','_')] = @{ packageName = $key; readmeName = $readme }
}

# 输出为 yaml
$matrixYaml = $matrix | ConvertTo-Yaml
Set-Content -Path $OutputFile -Value $matrixYaml
Write-Host "Matrix yaml generated at $OutputFile"
