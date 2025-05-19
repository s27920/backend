#!/bin/bash

cd fc-scripts
bash build-alp.sh
cd ..

bash /app/app-scripts/container-launch.sh

exec dotnet ExecutorService.dll