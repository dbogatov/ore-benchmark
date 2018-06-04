#!/usr/bin/env bash

set -e
shopt -s globstar

# Ensure that the CWD is set to script's location
cd "${0%/*}"
CWD=$(pwd)

rm -rf ../../coverage*

dotnet restore ./../../test/
dotnet build ./../../test/

dotnet restore

# Instrument assemblies inside 'test' folder to detect hits for source files inside 'src' folder
dotnet minicover instrument --workdir ../../ --assemblies test/**/bin/**/*.dll --sources src/**/*.cs

# Reset hits count in case minicover was run for this project
dotnet minicover reset

dotnet test --no-build --no-restore --verbosity n ./../../test/ --filter Category=Unit

# # Create HTML reports inside folder coverage-html
# # This command returns failure if the coverage is lower than the threshold
dotnet minicover htmlreport --workdir ../../

# # Print console report
# # This command returns failure if the coverage is lower than the threshold
# dotnet minicover report --workdir ../../ --threshold $threshold

# # Create NCover report
# dotnet minicover xmlreport --workdir ../ --threshold $threshold

# cd ..
