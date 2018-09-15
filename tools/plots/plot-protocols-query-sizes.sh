#!/usr/bin/env bash

set -e
shopt -s globstar

# Ensure that the CWD is set to script's location
cd "${0%/*}"
CWD=$(pwd)

values=( "ios" "vol" "size" )

set -x # echo ON

for value in "${values[@]}"
do
	./protocols-query-sizes.py $value
done

echo "Done!"
