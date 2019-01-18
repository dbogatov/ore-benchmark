#!/usr/bin/env bash

set -e
shopt -s globstar

# Ensure that the CWD is set to script's location
cd "${0%/*}"
CWD=$(pwd)

usage() { echo "Usage: $0 [-s <number>]" 1>&2; exit 1; }

SEED=$RANDOM

while getopts "s:" o; do
    case "${o}" in
        s)
            SEED=${OPTARG}
        	;;
        *)
            usage
        ;;
    esac
done
shift $((OPTIND-1))

dotnet build -c release -o dist/

schemes=( "clww" "bclo" "lewiwu" "fhope" "cloz" )

set -x # echo ON

for scheme in "${schemes[@]}"
do
    dotnet ./dist/schemes.dll \
		--data-dir ../../../data \
		--ore-scheme $scheme \
		--seed $SEED
done

echo "Done!"
