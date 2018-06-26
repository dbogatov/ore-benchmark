#!/usr/bin/env bash

set -e
shopt -s globstar

# Ensure that the CWD is set to script's location
cd "${0%/*}"
CWD=$(pwd)

usage() { echo "Usage: $0 [-s <number>]" 1>&2; exit 1; }

SCALE=0.001

while getopts "s:" o; do
	case "${o}" in
		s)
			SCALE=${OPTARG}
			;;
		*)
			usage
			;;
	esac
done
shift $((OPTIND-1))

cd "./tpc-h/"

make clean
make
./dbgen -T p -f -s "$SCALE"

echo "Done!"
