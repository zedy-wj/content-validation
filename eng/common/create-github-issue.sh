#!/bin/bash
set -e

ServiceName=$1
PackageName=$2
GitHubPat=$3
RepoOwner=$4
RepoName=$5

URL="https://api.github.com/repos/$RepoOwner/$RepoName/issues"
JSON="{
  \"title\": \"[$ServiceName - $PackageName] Content Validation Issue for learn microsoft website.\",
  \"body\": \"This is the body of the issue created by Azure DevOps Pipeline.\",
  \"labels\": [\"bug\"]
}"

response=$(curl -s -X POST -H "Authorization: token $GitHubPat" -H "Content-Type: application/json" -d "$JSON" "$URL")

echo "Response from GitHub API:"
echo "$response"