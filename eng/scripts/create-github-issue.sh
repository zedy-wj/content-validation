#!/bin/bash
set -e

# Setting Variables
PACKAGE_NAME=$1
GITHUB_PAT=$2
REPO_OWNER=$3
REPO_NAME=$4
ORG_NAME=$5
PROJECT_NAME=$6
RUN_ID=$7
# AZURE_DEVOPS_PAT=$8

ISSUE_TITLE="$PACKAGE_NAME content validation issue for learn microsoft website."
REPO_ROOT="$PWD"
RELATIVE_PATH="eng"
DIFF_ISSUE_FILE="$REPO_ROOT/$RELATIVE_PATH/GitHubBodyOrCommentDiff.txt"
TOTAL_ISSUE_FILE="$REPO_ROOT/$RELATIVE_PATH/GitHubBodyOrCommentTotal.txt"
STATUS_REPORTS_PATH="$REPO_ROOT/Reports/IssueStatusInfo.json"
ARTIFACT_NAME=$PACKAGE_NAME
API_VERSION="7.1-preview.5"

# AUTH_HEADER="-u :$AZURE_DEVOPS_PAT"
# URL="https://dev.azure.com/${ORG_NAME}/${PROJECT_NAME}/_apis/build/builds/${RUN_ID}/artifacts?artifactName=${ARTIFACT_NAME}&api-version=${API_VERSION}"

# # Send GET request
# ARTIFACT_INFO=$(curl -s -H "Accept: application/json" $AUTH_HEADER -X GET "$URL")
# download_url=$(echo "$ARTIFACT_INFO" | jq -r '.resource.downloadUrl')
# echo "Artifact download url: $download_url"


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
      # file_content="$file_content\n\nDetails of issue download url: $download_url"
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

else

  # Check whether the current pipeline have new issues of this package
  if [ -f "$DIFF_ISSUE_FILE" ]; then
      echo "$DIFF_ISSUE_FILE exists"
      file_content=$(cat "$DIFF_ISSUE_FILE")
      file_content="$file_content\n\nDetails of issue download url: $download_url"
      echo "$file_content"
  elif [ -f "$TOTAL_ISSUE_FILE" ]; then
      echo "$DIFF_ISSUE_FILE does not exist, have not new issues. But $TOTAL_ISSUE_FILE exist."
      exit 0
  else
      echo "$TOTAL_ISSUE_FILE does not exist, have not issues. Please remember close the issue manually in time!"
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
fi