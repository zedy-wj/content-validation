#!/bin/bash

GITHUB_PAT=$1
REPO_OWNER=$2
REPO_NAME=$3

REPO_URL="https://github.com/${REPO_OWNER}/${REPO_NAME}.git"
CLONE_DIR="./repo-clone"
CURRENT_DATE=$(date +"%Y-%m-%d")
BRANCH="trigger-$CURRENT_DATE"
GITHUB_TOKEN="${GITHUB_PAT}"
VALIDATE_MD_CONTENT="
| Header1 | Header2 | Header3 | Date_Time |
|---------|---------|---------|---------|
| Row1Col1| Row1Col2| Row1Col3| $CURRENT_DATE|
| Row2Col1| Row2Col2| Row2Col3| $CURRENT_DATE|
| Row3Col1| Row3Col2| Row3Col3| $CURRENT_DATE|
| Row4Col1| Row4Col2| Row4Col3| $CURRENT_DATE|"
 
git clone "https://${GITHUB_TOKEN}@github.com/${REPO_OWNER}/${REPO_NAME}.git" $CLONE_DIR
cd $CLONE_DIR
git checkout -b $BRANCH
 
FILE_NAME="pipline-result-${CURRENT_DATE}.md"

echo "$VALIDATE_MD_CONTENT" > "$FILE_NAME"
 
git add $FILE_NAME
git commit -m "Adding $FILE_NAME with a table"
 
git remote set-url origin "https://${GITHUB_TOKEN}@github.com/${REPO_OWNER}/${REPO_NAME}.git"
git push origin $BRANCH