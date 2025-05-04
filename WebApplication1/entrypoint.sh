#!/bin/sh
set -e
dockerd &
while ! docker info > /dev/null 2>&1; do
  echo "Waiting..."
  sleep 1
done
echo "daemon active"

exec dotnet WebApplication17.dll