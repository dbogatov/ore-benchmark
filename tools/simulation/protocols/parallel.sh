#!/usr/bin/env bash

set -e
shopt -s globstar

# Ensure that the CWD is set to script's location
cd "${0%/*}"
CWD=$(pwd)

usage() { echo "Usage: $0 [-s <number> -d <string> -q <string> -c <number>]" 1>&2; exit 1; }

SPACE=sandor-dolores-space

SEED=$RANDOM
QUERIES=1
CACHE=128

while getopts "s:q:c:" o; do
    case "${o}" in
        s)
			SEED=${OPTARG}
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

dotnet build -c release ../../../src/cli/ -o dist/

distros=( "uniform" "normal" "zipf" "employees" "forest" )

for distro in "${distros[@]}"
do
	{
		./protocols.sh -q $QUERIES -d $distro -c $CACHE -s $SEED -n
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
