#!/usr/bin/env bash

set -e
shopt -s globstar

# Ensure that the CWD is set to script's location
cd "${0%/*}"
CWD=$(pwd)

usage() { echo "Usage: $0 [-m <number> -s <number>]" 1>&2; exit 1; }

SEED=$RANDOM
MAX=100

while getopts "m:s:" o; do
	case "${o}" in
		m)
			MAX=${OPTARG}
			;;
		s)
			SEED=${OPTARG}
			;;
		*)
			usage
			;;
	esac
done
shift $((OPTIND-1))

dotnet build -c release ../src/data-gen/

set -x # echo ON
dotnet ../src/data-gen/bin/release/netcoreapp2.0/data-gen.dll --dataset --count $MAX --max $MAX --seed $SEED > dataset.txt

queries=( exact range update delete )
for query in "${queries[@]}"
do
	dotnet ../src/data-gen/bin/release/netcoreapp2.0/data-gen.dll --queries-type $query --count $MAX --max $MAX --seed $SEED > $query-queries.txt
done

echo "Done!"
