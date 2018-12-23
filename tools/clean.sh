#!/usr/bin/env bash

set -e
shopt -s globstar

# Ensure that the CWD is set to script's location
cd "${0%/*}"
CWD=$(pwd)

rm -rf ../src/*/{dist,obj,bin}
rm -rf ../src/ore-schemes/**/{dist,obj,bin}
rm -rf ../test/{dist,obj,bin}

rm -rf ../tools/**/{dist,obj,bin}

echo "Done!"
