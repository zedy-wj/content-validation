#!/bin/bash
set -e

# Setting Variables
SERVICE_NAME=$1
PACKAGE_NAME=$2
GITHUB_PAT=$3
REPO_OWNER=$4
REPO_NAME=$5
ISSUE_TITLE="[$SERVICE_NAME - $PACKAGE_NAME] Content Validation Issue for learn microsoft website."

# Querying whether issue exist
QUERY_URL="https://api.github.com/search/issues?q=repo:$REPO_OWNER/$REPO_NAME+state:open+$ISSUE_TITLE&per_page=1"

url_encode(){
  local encoded="${1// /%20}"
  encoded="${encoded//\[/%5B}"
  encoded="${encoded//\]/%5D}"
  echo $encoded
}

QUERY_URL=$(url_encode "$QUERY_URL")
# Getting response
response=$(curl -s \
  -H "Accept: application/vnd.github.v3+json" \
  -H "Authorization: token $GITHUB_PAT" "$QUERY_URL")
# Parsing the response
item_count=$(echo "$response" | jq -r '.total_count')

if [ "$item_count" -eq 0 ]; then
  # Opening new issue
  URL="https://api.github.com/repos/$REPO_OWNER/$REPO_NAME/issues"
  JSON="{
    \"title\": \"$ISSUE_TITLE\",
    \"body\": \"This is the body of the issue created by Azure DevOps Pipeline.\",
    \"labels\": [\"bug\"]
  }"

  response=$(curl -s -X POST -H "Authorization: token $GITHUB_PAT" -H "Content-Type: application/json" -d "$JSON" "$URL")

  echo "Response from GitHub API:"
  echo "$response"

else
  echo "Issue already exist."
fi