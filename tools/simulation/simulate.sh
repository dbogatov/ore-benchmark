#!/usr/bin/env bash

set -e
shopt -s globstar

# Ensure that the CWD is set to script's location
cd "${0%/*}"
CWD=$(pwd)

SIMULATION=false
SPACE="jorah"

usage() { echo "Usage: $0 [-p -s -b -r -S]" 1>&2; exit 1; }

while getopts "psbrS" o; do
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
		S)
			rm -rf ./../../results/schemes-sim
			mkdir -p ./../../results/schemes-sim/
			./schemes/schemes.sh 2>&1 | tee ./../../results/schemes-sim/out.script
			cp -r ./../../data ./../../results/schemes-sim/
			SIMULATION=true
			s3cmd -c config put --recursive ./../../results/schemes-sim/* s3://$SPACE/public/ore-sim-results/schemes-sim/$(date +"%Y-%m-%d_%H-%M-%S")/
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
