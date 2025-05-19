#!/bin/bash

docker run -p "$1":"$1" compiler & disown