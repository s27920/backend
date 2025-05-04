#!/bin/sh
# arg1 = container name
name="$1"
docker stop --time=5 "$name"
if [ $? -ne 0 ]; then 
    docker kill "$name"
fi
