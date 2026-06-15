#!/bin/bash

echo "Checking if build should run..."

if [[ "$VERCEL_GIT_COMMIT_REF" == "main" ]] || [[ "$VERCEL_GIT_COMMIT_MESSAGE" == *"[build]"* ]]; then
  echo "Proceeding with build"
    exit 1
    else
      echo "Ignoring build"
        exit 0
        fi
        
