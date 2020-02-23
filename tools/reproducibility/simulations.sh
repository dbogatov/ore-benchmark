#!/usr/bin/env bash

set -e
shopt -s globstar

cd /benchmark/

dotnet ./src/cli/dist/cli.dll $@
