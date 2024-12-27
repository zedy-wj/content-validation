#!/bin/bash
set -e

# Setting Variables
GITHUB_PAT=$1
REPO_OWNER=$2
REPO_NAME=$3
GIT_USER_NAME=$4
GIT_USER_EMAIL=$5

REPO_URL="https://github.com/${REPO_OWNER}/${REPO_NAME}.git"
CLONE_DIR="./repo-clone"
CURRENT_DATE=$(date +"%Y-%m-%d")
BRANCH="trigger-$CURRENT_DATE"
FILE_NAME="pipline-result-$CURRENT_DATE.md"

cat $FILE_NAME
git clone "https://${GITHUB_PAT}@github.com/${REPO_OWNER}/${REPO_NAME}.git" $CLONE_DIR
cp $FILE_NAME $CLONE_DIR
cd $CLONE_DIR
git checkout -b $BRANCH
git config --global user.email "${GIT_USER_EMAIL}"
git config --global user.name "${GIT_USER_NAME}"

git add $FILE_NAME
git commit -m "Adding $FILE_NAME with a table"

git remote set-url origin "https://${GITHUB_PAT}@github.com/${REPO_OWNER}/${REPO_NAME}.git"
git push origin $BRANCH