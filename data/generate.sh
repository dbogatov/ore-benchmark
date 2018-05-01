#!/usr/bin/env bash

set -e
shopt -s globstar

# Ensure that the CWD is set to script's location
cd "${0%/*}"
CWD=$(pwd)

usage() { echo "Usage: $0 [-m <number> -s <number> -n]" 1>&2; exit 1; }

SEED=$RANDOM
MAX=100
BUILD=true

while getopts "m:s:n" o; do
	case "${o}" in
		m)
			MAX=${OPTARG}
			;;
		s)
			SEED=${OPTARG}
			;;
		n)
			BUILD=false
			;;
		*)
			usage
			;;
	esac
done
shift $((OPTIND-1))

if [ "$BUILD" == true ];
then
	dotnet build -c release ../tools/data-gen/ -o dist/
fi

set -x # echo ON
dotnet ../tools/data-gen/dist/data-gen.dll --dataset --count $MAX --max $MAX --seed $SEED > dataset.txt

queries=( exact range update delete )
for query in "${queries[@]}"
do
	dotnet ../tools/data-gen/dist/data-gen.dll --queries-type $query --count $MAX --max $MAX --seed $SEED > $query-queries.txt
done

echo "Done!"
