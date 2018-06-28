#!/usr/bin/env bash

set -e
shopt -s globstar

# Ensure that the CWD is set to script's location
cd "${0%/*}"
CWD=$(pwd)

usage() { echo "Usage: $0 <flag>" 1>&2; exit 1; }

if [ $# -eq 0 ]
then
    usage
fi

cd ../../src/benchmark/

echo "You may be promted for sudo password."
echo "This is needed to set process' priority and remove old artifacts."

sudo rm -rf BenchmarkDotNet.Artifacts
sudo dotnet run -c RELEASE -- "--$1"

echo "Done!"
