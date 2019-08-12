#!/usr/bin/env bash

set -e
shopt -s globstar

# Ensure that the CWD is set to script's location
cd "${0%/*}"
CWD=$(pwd)

usage() { echo "Usage: $0 [-d <number> -q <number> -s <number> -n]" 1>&2; exit 1; }

SEED=$RANDOM
QMAX=100
DMAX=100
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

mkdir -p ../../data

if [ "$BUILD" == true ];
then
	types=( "uniform" "normal" "zipf" "employees" "forest" )
else
	types=( "uniform" )
fi

set -x # echo ON

for type in "${types[@]}"
do
	mkdir -p ../../data/$type
	dotnet ./dist/data-gen.dll \
		--type $type \
		--output ../../data/$type \
		--data-size $DMAX \
		--normal-stddev $(( $DMAX / 10 )) \
		--uniform-range $(( $DMAX / 3 )) \
		--query-size $QMAX \
		--seed $SEED \
		--employees-url "https://gist.github.com/dbogatov/a192d00d72de02f188c5268ea1bbf25b/raw/afb4d44111f42dbe9ed0148bee3b954d43e38907/state-of-california-2017.csv" \
		--forest-url "https://gist.github.com/dbogatov/a192d00d72de02f188c5268ea1bbf25b/raw/afb4d44111f42dbe9ed0148bee3b954d43e38907/covtype.data"
done

echo "Done!"
