#!/usr/bin/env bash

set -e
shopt -s globstar

# Ensure that the CWD is set to script's location
cd "${0%/*}"
CWD=$(pwd)

if [ -n "$1" ]
then
	dotnet build --no-restore

	echo "Running test $1 ..."
	dotnet test --no-build --no-restore --filter "FullyQualifiedName~$1"
else
	dotnet restore --disable-parallel
	dotnet build --no-restore

	echo "Running dotnet tests..."
	dotnet test --no-build --no-restore --verbosity n
fi

echo "Testing completed!"
