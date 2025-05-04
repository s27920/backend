#!/bin/sh

ext=$1 #file type 
filename=$2
cat > "/app/${filename}.${ext}" 