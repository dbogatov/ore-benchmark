#!/usr/bin/env bash

set -e
shopt -s globstar

# Ensure that the CWD is set to script's location
cd "${0%/*}"
CWD=$(pwd)

usage() { echo "Usage: $0 [-c <string> -n <string>]" 1>&2; exit 1; }

NAME=""
CATEGORY=""

while getopts "n:c:" o; do
	case "${o}" in
		n)
			NAME="--filter FullyQualifiedName~${OPTARG}"
			;;
		c)
			CATEGORY="--filter Category=${OPTARG}"
			;;
		*)
			usage
			;;
	esac
done
shift $((OPTIND-1))

dotnet restore --disable-parallel
dotnet build --no-restore

echo "Running dotnet tests..."

dotnet test --no-build --no-restore --verbosity n $NAME $CATEGORY

echo "Testing completed!"
