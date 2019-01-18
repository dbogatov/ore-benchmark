#!/usr/bin/env bash

set -e
shopt -s globstar

# Ensure that the CWD is set to script's location
cd "${0%/*}"
CWD=$(pwd)

usage() { echo "Usage: $0 [-s <number> -d <string> -q <string> -c <number> -n]" 1>&2; exit 1; }

SEED=$RANDOM
DATA=uniform
QUERIES=1
CACHE=128
BUILD=true
DATAPERCENT=100
VERBOSE=""

while getopts "p:s:d:q:c:nv" o; do
    case "${o}" in
        s)
			SEED=${OPTARG}
			;;
		v)
			VERBOSE="--extended"
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
		p)
			DATAPERCENT=${OPTARG}
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

declare -A protocols # Create an associative array
protocols[clww]=512
protocols[bclo]=512 
protocols[fhope]=512 
protocols[lewiwu]=11 
protocols[cloz]=8 
protocols[kerschbaum]=256 
protocols[pope]=256
protocols[popecold]=256 
protocols[noencryption]=1024

mkdir -p ../../../results/protocols

if [ "$BUILD" == true ];
then
	dotnet build -c release ../../../src/cli/ -o dist/
fi

set -x # echo ON

for protocol in "${!protocols[@]}"
do
	TOEXECUTE=$protocol

	QPREFIX=""
	if [ "$protocol" == "popecold" ]
	then
		TOEXECUTE="pope"
		QPREFIX="mini-"
	fi

	echo "Current timestamp: $(date)"
	dotnet ../../../src/cli/dist/cli.dll $VERBOSE \
		--dataset ../../../data/$DATA/data.txt \
		--ore-scheme $TOEXECUTE \
		--seed $SEED \
		protocol \
		--queries ../../../data/$DATA/${QPREFIX}queries-$QUERIES.txt \
		--cache-size $CACHE \
		--b-plus-tree-branches ${protocols[$protocol]} \
		--data-percent $DATAPERCENT \
		> ../../../results/protocols/$protocol-$DATA-$QUERIES-$DATAPERCENT-$CACHE-$SEED.json
done

echo "Done!"
