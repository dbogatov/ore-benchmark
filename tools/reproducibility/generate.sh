#!/usr/bin/env bash

set -e
shopt -s globstar

/benchmark/tools/data-gen/generate.sh $@
