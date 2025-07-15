# PowerShell version of push-pipeline-result-markdown.sh for Windows compatibility
param(
    [Parameter(Mandatory=$true)]
    [string]$GitHubPat,
    
    [Parameter(Mandatory=$true)]
    [string]$RepoOwner,
    
    [Parameter(Mandatory=$true)]
    [string]$RepoName,
    
    [Parameter(Mandatory=$true)]
    [string]$GitUserName,
    
    [Parameter(Mandatory=$true)]
    [string]$GitUserEmail,
    
    [Parameter(Mandatory=$true)]
    [string]$Language
)

# Set error action preference to stop on errors
$ErrorActionPreference = "Stop"

try {
    Write-Host "=== Starting Git operations ==="
    
    # Convert language to lowercase
    $language = $Language.ToLower()
    
    # Setting Variables
    $repoUrl = "https://github.com/$RepoOwner/$RepoName.git"
    $cloneDir = "./repo-clone"
    $branch = "latest-pipeline-result"
    $fileName = "latest-pipeline-result-for-$language.md"
    
    Write-Host "Repository: $repoUrl"
    Write-Host "Clone Directory: $cloneDir"
    Write-Host "Branch: $branch"
    Write-Host "File Name: $fileName"
    
    # Remove existing clone directory if it exists
    if (Test-Path $cloneDir) {
        Write-Host "Removing existing clone directory..."
        Remove-Item -Path $cloneDir -Recurse -Force
    }
    
    # Clone the repository
    Write-Host "Cloning repository..."
    $cloneUrl = "https://$GitHubPat@github.com/$RepoOwner/$RepoName.git"
    git clone $cloneUrl $cloneDir
    
    if ($LASTEXITCODE -ne 0) {
        throw "Git clone failed with exit code $LASTEXITCODE"
    }
    
    # Change to clone directory
    Set-Location $cloneDir
    
    # Checkout and pull the branch
    Write-Host "Checking out branch $branch..."
    git checkout $branch
    
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Branch $branch doesn't exist, creating it..."
        git checkout -b $branch
    }
    
    git pull origin $branch
    
    # Copy the file
    $sourceFile = "../$fileName"
    if (Test-Path $sourceFile) {
        Write-Host "Copying file $sourceFile to current directory..."
        Copy-Item -Path $sourceFile -Destination . -Force
    } else {
        Write-Warning "Source file $sourceFile not found, creating empty file..."
        "# Latest Pipeline Result for $Language" | Out-File -FilePath $fileName -Encoding UTF8
    }
    
    # Configure git
    Write-Host "Configuring git user..."
    git config --global user.email $GitUserEmail
    git config --global user.name $GitUserName
    
    # Add, commit and push
    Write-Host "Adding file to git..."
    git add $fileName
    
    Write-Host "Committing changes..."
    git commit -m "Updating the latest pipeline result"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "No changes to commit"
    } else {
        Write-Host "Pushing changes..."
        git push origin $branch
        
        if ($LASTEXITCODE -ne 0) {
            throw "Git push failed with exit code $LASTEXITCODE"
        }
    }
    
    Write-Host "✓ Git operations completed successfully"
    
} catch {
    Write-Error "❌ Error during git operations: $_"
    exit 1
} finally {
    # Return to original directory
    if (Test-Path "../") {
        Set-Location ".."
    }
    
    # Clean up clone directory
    if (Test-Path $cloneDir) {
        Write-Host "Cleaning up clone directory..."
        Remove-Item -Path $cloneDir -Recurse -Force -ErrorAction SilentlyContinue
    }
}
