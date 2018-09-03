#!/usr/bin/env bash

set -e
shopt -s globstar

# Ensure that the CWD is set to script's location
cd "${0%/*}"
CWD=$(pwd)

BUILD=false
PUBLISH=false
KEY=""

usage() { echo "Usage: $0 <-b | -p> -k <key>]" 1>&2; exit 1; }

while getopts "pbk:" o; do
	case "${o}" in
		p)
			PUBLISH=true
			;;
		b)
			BUILD=true
			;;
		k)
			KEY=${OPTARG}
			;;
		*)
			usage
			;;
	esac
done
shift $((OPTIND-1))

if [ "$KEY" == "" ];
then
	usage
	exit 1
fi

declare -A PROJECTS
PROJECTS['ore-schemes/shared']='ore-benchamrk.shared'
PROJECTS['b-plus-tree']='b-plus-tree'

cd ..
VERSION=$(<version.txt)

if [ "$BUILD" == true ]
then

	# Update all versions
	sed -i -e "s#<Version>.*</Version>#<Version>$VERSION</Version>#g" ./src/**/*.csproj && rm -f ./src/**/*.csproj-e
	sed -i -e "s#<Version>.*</Version>#<Version>$VERSION</Version>#g" ./src/ore-schemes/**/*.csproj && rm -f ./src/ore-schemes/**/*.csproj-e

	# Build packages
	for project in "${!PROJECTS[@]}"
	do
		dotnet pack ./src/$project/ -o ./dist/
	done
fi

if [ "$PUBLISH" == true ]
then

	# Push packages
	for project in "${!PROJECTS[@]}"
	do
		dotnet nuget push "./src/$project/dist/${PROJECTS[$project]}.$VERSION.nupkg" -k "$KEY"  -s "https://api.nuget.org/v3/index.json" 
	done

fi

echo "Done!"
