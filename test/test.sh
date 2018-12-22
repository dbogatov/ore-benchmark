#!/usr/bin/env bash

set -e
shopt -s globstar

# Ensure that the CWD is set to script's location
cd "${0%/*}"
CWD=$(pwd)

usage() { echo "Usage: $0 [-c <string> -n <string> -j]" 1>&2; exit 1; }

NAME=""
CATEGORY=""
JUNIT=""

while getopts "n:c:j" o; do
	case "${o}" in
		j)
			JUNIT="--logger trx"
			;;
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

echo "Changing environment to testing..."
export ASPNETCORE_ENVIRONMENT="Testing"

dotnet test --no-build --no-restore --verbosity n $JUNIT $NAME $CATEGORY

echo "Testing completed!"
