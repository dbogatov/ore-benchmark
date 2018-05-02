#!/usr/bin/env bash

set -e
shopt -s globstar

# Ensure that the CWD is set to script's location
cd "${0%/*}"
CWD=$(pwd)

usage() { echo "Usage: $0 [-d <number> -q <number> -s <number> -n]" 1>&2; exit 1; }

SEED=$RANDOM
DMAX=100
QMAX=100
BUILD=true

while getopts "d:q:s:n" o; do
	case "${o}" in
		d)
			DMAX=${OPTARG}
			;;
		q)
			QMAX=${OPTARG}
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
	dotnet build -c release -o dist/
fi

mkdir -p ../../data/d

set -x # echo ON
dotnet ./dist/data-gen.dll --dataset --count $DMAX --max $DMAX --seed $SEED > ../../data/dataset.txt

queries=( exact update delete )
for query in "${queries[@]}"
do
	dotnet ./dist/data-gen.dll --queries-type $query --count $QMAX --max $DMAX --seed $SEED > ../../data/$query-queries.txt
done

ranges=( 0.5 1 2 3 )
for range in "${ranges[@]}"
do
	dotnet ./dist/data-gen.dll --queries-type range --count $QMAX --max $DMAX --range-percent $range --seed $SEED > ../../data/range-$range-queries.txt
done

echo "Done!"
