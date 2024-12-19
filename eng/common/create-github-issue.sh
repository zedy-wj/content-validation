#!/bin/bash
set -e

# Setting Variables
PACKAGE_NAME=$1
GITHUB_PAT=$2
REPO_OWNER=$3
REPO_NAME=$4
ISSUE_TITLE="$PACKAGE_NAME content validation issue for learn microsoft website."

REPO_ROOT="$PWD"
RELATIVE_PATH="Reports"
SPECIFIC_FILE="$REPO_ROOT/$RELATIVE_PATH/ReportResults.txt"

if [ -f "$SPECIFIC_FILE" ]; then
    echo "$SPECIFIC_FILE exists"
    file_content=$(cat "$SPECIFIC_FILE")
    echo "$file_content"
else
    echo "$SPECIFIC_FILE does not exist, have not new issues."
    exit 0
fi

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
  create_new_issue="https://api.github.com/repos/$REPO_OWNER/$REPO_NAME/issues"
  json="{
    \"title\": \"$ISSUE_TITLE\",
    \"body\": \"$file_content\",
    \"labels\": [\"bug\"]
  }"

  response=$(curl -s -X POST -H "Authorization: token $GITHUB_PAT" -H "Content-Type: application/json" -d "$json" "$create_new_issue")

  echo "Response from GitHub API:"
  echo "$response"

else
  echo "Issue already exist. Adding new comment to track new issues."
  issue_number=$(echo "$response" | jq -r '.items[0].number')
  add_comment_url="https://api.github.com/repos/$REPO_OWNER/$REPO_NAME/issues/$issue_number/comments"
  response=$(curl -X POST \
    -H "Accept: application/vnd.github.v3+json" \
    -H "Authorization: token $GITHUB_PAT" \
    $add_comment_url \
    -d "{
      \"body\": \"$file_content\"
    }")
fi