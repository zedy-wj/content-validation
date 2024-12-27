#!/bin/bash
set -e

# Setting Variables
GITHUB_PAT=$1
REPO_OWNER=$2
REPO_NAME=$3

REPO_URL="https://github.com/${REPO_OWNER}/${REPO_NAME}.git"
CLONE_DIR="./repo-clone"
CURRENT_DATE=$(date +"%Y-%m-%d")
BRANCH="trigger-$CURRENT_DATE"
VALIDATE_MD_CONTENT="
| id | package | status | issue link | created date of issue | update date of issue |
|---------|---------|---------|---------|---------|---------|
| 1 | PackageA: azure-appconfiguration | PASS | IssueLink | $CURRENT_DATE | $CURRENT_DATE |
| 2 | PackageB: azure-keyvault-keys | FAIL | IssueLink | $CURRENT_DATE | $CURRENT_DATE |

git clone "https://${GITHUB_PAT}@github.com/${REPO_OWNER}/${REPO_NAME}.git" $CLONE_DIR
cd $CLONE_DIR
git checkout -b $BRANCH
 
FILE_NAME="pipline-result-${CURRENT_DATE}.md"

echo "$VALIDATE_MD_CONTENT" > "$FILE_NAME"
 
git add $FILE_NAME
git commit -m "Adding $FILE_NAME with a table"
 
git remote set-url origin "https://${GITHUB_PAT}@github.com/${REPO_OWNER}/${REPO_NAME}.git"
git push origin $BRANCH