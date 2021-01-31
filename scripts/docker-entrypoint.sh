#!/bin/bash

# Exit if any command has a non-zero exit status (Exists if a command returns an exception, like it's a programming language)
# Prevents errors in a pipeline from being hidden. So if any command fails, that return code will be used as the return code of the whole pipeline
set -eo pipefail

replace-librdkafka.sh

check-environment.sh

#Traps if the process is stopped before the .dll is executed
trap "exit 0" SIGTERM SIGINT
#.dll handles exit code once started.
exec dotnet "$DOTNET_PROGRAM_HOME"/DashboardServer.dll