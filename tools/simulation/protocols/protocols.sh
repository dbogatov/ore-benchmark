#!/usr/bin/env bash

set -e
shopt -s globstar

# Ensure that the CWD is set to script's location
cd "${0%/*}"
CWD=$(pwd)

usage() { echo "Usage: $0 [-s <number> -d <string> -q <string> -c <number>]" 1>&2; exit 1; }

SEED=$RANDOM
DATA=uniform
QUERIES=1
CACHE=128

while getopts "s:d:q:c:" o; do
    case "${o}" in
        s)
			SEED=${OPTARG}
			;;
		d)
			DATA=${OPTARG}
        	;;
		q)
			QUERIES=${OPTARG}
        	;;
		c)
			CACHE=${OPTARG}
        	;;
        *)
            usage
        ;;
    esac
done
shift $((OPTIND-1))

declare -A protocols     # Create an associative array
protocols[practicalore]=512
protocols[cryptdb]=512 
protocols[fhope]=512 
protocols[lewiore]=11 
protocols[adamore]=8 
protocols[florian]=256 
protocols[pope]=256 

 mkdir -p ../../../results/protocols

set -x # echo ON

dotnet build -c release ../../../src/cli/ -o dist/

for protocol in "${!protocols[@]}"
do
	echo "Current timestamp: $(date)"
	dotnet ../../../src/cli/dist/cli.dll \
		--dataset ../../../data/$DATA/data.txt \
		--ore-scheme $protocol \
		--seed $SEED \
		protocol \
		--queries ../../../data/$DATA/queries-$QUERIES.txt \
		--cache-size $CACHE \
		--b-plus-tree-branches ${protocols[$protocol]} > ../../../results/protocols/$protocol-$DATA-$QUERIES-$CACHE-$SEED.json
done

echo "Done!"
