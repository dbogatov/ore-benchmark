#!/usr/bin/env bash

set -e
shopt -s globstar

# Ensure that the CWD is set to script's location
cd "${0%/*}"
CWD=$(pwd)

# See https://github.com/lucaslorentz/minicover

rm -rf ../../coverage*

dotnet restore ./../../test/
dotnet build ./../../test/

dotnet restore

# Instrument assemblies inside 'test' folder to detect hits for source files inside 'src' folder
dotnet minicover instrument --workdir ../../ --assemblies test/**/bin/**/*.dll --sources src/**/*.cs --sources test/**/*.cs

# Reset hits count in case minicover was run for this project
dotnet minicover reset

dotnet test --no-build --no-restore --verbosity n ./../../test/ --filter Category=Unit || true

# Uninstrument assemblies, it's important if you're going to publish or deploy build outputs
dotnet minicover uninstrument --workdir ../../

dotnet minicover report --workdir ../../
dotnet minicover htmlreport --workdir ../../
