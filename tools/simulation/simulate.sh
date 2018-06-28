#!/usr/bin/env bash

set -e
shopt -s globstar

# Ensure that the CWD is set to script's location
cd "${0%/*}"
CWD=$(pwd)

SIMULATION=false
SPACE="denis-dolores-space"

usage() { echo "Usage: $0 [-p -s -b -r]" 1>&2; exit 1; }

while getopts "psbr" o; do
	case "${o}" in
		p)
			./protocol.rb
			SIMULATION=true
			s3cmd -c config put ./../../results/protocol.json s3://$SPACE/public/ore-sim-results/protocol/$(date +"%Y-%m-%d_%H-%M-%S").json
			;;
		s)
			./pure-schemes.rb
			SIMULATION=true
			s3cmd -c config put ./../../results/schemes.json s3://$SPACE/public/ore-sim-results/schemes/$(date +"%Y-%m-%d_%H-%M-%S").json
			;;
		b)
			./benchmark.sh "schemes"
			SIMULATION=true
			s3cmd -c config put --recursive ./../../src/benchmark/BenchmarkDotNet.Artifacts/* s3://$SPACE/public/ore-sim-results/benchmark/$(date +"%Y-%m-%d_%H-%M-%S")/
			;;
		r)
			./benchmark.sh "primitives"
			SIMULATION=true
			s3cmd -c config put --recursive ./../../src/benchmark/BenchmarkDotNet.Artifacts/* s3://$SPACE/public/ore-sim-results/primitives/$(date +"%Y-%m-%d_%H-%M-%S")/
			;;
		*)
			usage
			;;
	esac
done
shift $((OPTIND-1))

if [ "$SIMULATION" == false ];
then
	usage
	exit 1
fi

echo "Done!"
