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
PROJECTS['crypto/shared']='ore-benchamrk.shared'
PROJECTS['b-plus-tree']='b-plus-tree'
PROJECTS['crypto/clww-ore']='clww-ore'
PROJECTS['crypto/lewi-wu-ore']='lewi-wu-ore'
PROJECTS['crypto/fh-ope']='fh-ope'
PROJECTS['crypto/bclo-ope']='bclo-ope'
PROJECTS['crypto/cjjkrs-sse']='cjjkrs-sse'
PROJECTS['crypto/cjjjkrs-sse']='cjjjkrs-sse'

cd ..
VERSION=$(<version.txt)

if [ "$BUILD" == true ]
then
	# Build packages
	for project in "${!PROJECTS[@]}"
	do
		dotnet pack ./src/$project/ /p:PackageVersion=$VERSION -o ./dist/
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
