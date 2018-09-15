#!/usr/bin/env bash

set -e
shopt -s globstar

# Ensure that the CWD is set to script's location
cd "${0%/*}"
CWD=$(pwd)

usage() { echo "Usage: $0 [-s <number> -d <string> -q <string> -c <number>]" 1>&2; exit 1; }

SPACE=vadim-dolores-space

SEED=$RANDOM
DATA="uniform"
CACHE=128
QUERIES=1

while getopts "s:c:d:q:" o; do
    case "${o}" in
        s)
			SEED=${OPTARG}
			;;
		c)
			CACHE=${OPTARG}
        	;;
		d)
			DATA=${OPTARG}
			;;
		q)
			QUERIES=${OPTARG}
        	;;
        *)
            usage
        ;;
    esac
done
shift $((OPTIND-1))

dotnet build -c release ../../../src/cli/ -o dist/

datapercents=( "5" "10" "20" "50" "100" )

for datapercent in "${datapercents[@]}"
do
	{
		./protocols.sh -q $QUERIES -d $DATA -p $datapercent -c $CACHE -s $SEED -n
	} &
	PIDS+=($!)
done

sleep 1

echo "Running all tasks in background. Waiting."

for PID in "${PIDS[@]}"
do
	wait ${PID}
done

echo "All tasks completed successfully."

s3cmd -c ../config put --recursive ../../../results/protocols/*$SEED* s3://$SPACE/public/ore-sim-results/protocols-sim/$(date +"%Y-%m-%d_%H-%M-%S")/

echo "Done!"
