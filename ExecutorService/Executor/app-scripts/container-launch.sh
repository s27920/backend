#!/bin/bash

# TODO read these from .env
CONTAINER_COUNT=5
BASE_PORT=5137
HOST="172.21.40.155"

for i in $(seq 0 $(($CONTAINER_COUNT-1)));
do
  PORT=$(($BASE_PORT+$i))
  if [ $( docker ps | grep -c ":::$PORT->$BASE_PORT/tcp" ) -gt 0 ]; then
    docker kill $(docker ps | grep ":::$PORT->$BASE_PORT/tcp" | cut -d ' ' -f1)
    echo "killed: $PORT"
  fi
  docker run -p $PORT:$BASE_PORT compiler & disown 
done

CONTINUE_FLAG=0
while [ $CONTINUE_FLAG -eq 0 ]; 
do
  CONTINUE_FLAG=1
  for i in $(seq 0 $(($CONTAINER_COUNT-1)));
  do
    PORT=$(($BASE_PORT+$i))
    if ! nc -z $HOST $PORT; then
      CONTINUE_FLAG=0
      break
    fi  
  done
  sleep 1
done

echo "services ready"