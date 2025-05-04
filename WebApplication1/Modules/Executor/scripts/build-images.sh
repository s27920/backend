#!/bin/sh

x=0
echo "building images..."
for var in "$@"
do
  docker build -q -t "$var"-executor -f executor-images/"$var"-image.dockerfile .
  x=$(( x + $? ))
done

echo "build completed with: ${x} errors"