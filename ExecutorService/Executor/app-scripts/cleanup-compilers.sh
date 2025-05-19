#!/bin/bash

for i in $(docker ps | grep "compiler" | cut -d ' ' -f1); do docker kill $i; done