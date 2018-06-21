#!/usr/bin/env bash

set -e
shopt -s globstar

# Ensure that the CWD is set to script's location
cd "${0%/*}"
CWD=$(pwd)

SIMULATION=""

usage() { echo "Usage: $0 [-p -s]" 1>&2; exit 1; }

while getopts "ps" o; do
	case "${o}" in
		p)
			./protocol.rb
			SIMULATION="protocol"
			;;
		s)
			./pure-schemes.rb
			SIMULATION="schemes"
			;;
		*)
			usage
			;;
	esac
done
shift $((OPTIND-1))

if [ "$SIMULATION" == "" ];
then
	usage
	exit 1
fi

s3cmd -c config put ./../../results/$SIMULATION.json s3://$SPACE/public/ore-sim-results/$(date +"%Y-%m-%d_%H-%M-%S").json

echo "Done!"
