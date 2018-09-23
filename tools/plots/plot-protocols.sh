#!/usr/bin/env bash

set -e
shopt -s globstar

# Ensure that the CWD is set to script's location
cd "${0%/*}"
CWD=$(pwd)

values=( "ios" "vol" "size" )
stages=( "c" "q")

set -x # echo ON

for value in "${values[@]}"
do
	for stage in "${stages[@]}"
	do
		./protocols-charts.py $stage$value
	done
done

echo "Done!"
