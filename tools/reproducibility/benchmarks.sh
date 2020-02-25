#!/usr/bin/env bash

set -e
shopt -s globstar

cd /benchmark/src/benchmark/

dotnet run -- $@
