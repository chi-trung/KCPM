#!/bin/bash
# Vercel Ignored Build Step
# Exit 0 (skip build) for gh-pages branch
# Exit 1 (proceed with build) for main branch

if [ "$VERCEL_GIT_COMMIT_REF" = "gh-pages" ]; then
  echo ">> Skipping build for gh-pages branch"
  exit 0
fi

echo ">> Proceeding with build for branch: $VERCEL_GIT_COMMIT_REF"
exit 1
