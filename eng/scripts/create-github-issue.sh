#!/bin/bash
set -e

# Setting Variables
PACKAGE_NAME=$1
GITHUB_PAT=$2
REPO_OWNER=$3
REPO_NAME=$4
ISSUE_TITLE="$PACKAGE_NAME content validation issue for learn microsoft website."

REPO_ROOT="$PWD"
RELATIVE_PATH="eng"
DIFF_ISSUE_FILE="$REPO_ROOT/$RELATIVE_PATH/GitHubBodyOrCommentDiff.txt"
TOTAL_ISSUE_FILE="$REPO_ROOT/$RELATIVE_PATH/GitHubBodyOrCommentTotal.txt"
STATUS_REPORTS_PATH="$REPO_ROOT/Reports/IssueStatusInfo.json"

# record issue status method
record_issue_status() {
  response=$1
  status=$2
  status_reports_path=$3

  echo "$response" | jq -r "{
    \"status\": \"$status\",
    \"url\": .items[0].url, 
    \"created_at\": .items[0].created_at, 
    \"updated_at\": .items[0].updated_at
  }" > $status_reports_path
}

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

if [ -z "$item_count" ]; then
  echo "$response"
  exit 1
elif [ $item_count -eq 0 ]; then

  # Check whether the current pipeline have issues of this package
  if [ -f "$TOTAL_ISSUE_FILE" ]; then
      echo "$TOTAL_ISSUE_FILE exists"
      file_content=$(cat "$TOTAL_ISSUE_FILE")
  else
      echo "$TOTAL_ISSUE_FILE does not exist, have not issues."
      exit 0
  fi

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

  echo "$response" | jq -r "{
    \"status\": \"Fail\",
    \"url\": .url, 
    \"created_at\": .created_at, 
    \"updated_at\": .updated_at
  }" > $STATUS_REPORTS_PATH

else

  # Check whether the current pipeline have new issues of this package
  if [ -f "$DIFF_ISSUE_FILE" ]; then
      echo "$DIFF_ISSUE_FILE exists"
      file_content=$(cat "$DIFF_ISSUE_FILE")
      echo "$file_content"
  elif [ -f "$TOTAL_ISSUE_FILE" ]; then
      echo "$DIFF_ISSUE_FILE does not exist, have not new issues. But $TOTAL_ISSUE_FILE exist, recording the issue status."
      record_issue_status "$response" "Fail" "$STATUS_REPORTS_PATH"
      exit 0
  else
      echo "$TOTAL_ISSUE_FILE does not exist, have not issues. Please remember close the issue manually in time!"
      record_issue_status "$response" "Pass" "$STATUS_REPORTS_PATH"
      exit 0
  fi

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
  echo "$response"

  # Getting the latest response
  response=$(curl -s \
    -H "Accept: application/vnd.github.v3+json" \
    -H "Authorization: token $GITHUB_PAT" "$QUERY_URL")

  record_issue_status "$response" "Fail" "$STATUS_REPORTS_PATH"
fi
