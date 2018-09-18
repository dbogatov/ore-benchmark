#!/usr/bin/env bash

set -e
shopt -s globstar

# Ensure that the CWD is set to script's location
cd "${0%/*}"
CWD=$(pwd)

set -x # echo ON

# schemes
dotnet run --type schemes --file ../../../src/benchmark/BenchmarkDotNet.Artifacts/results/*.json > ./../../plots/data/schemes-benchmark.txt
cd ../../plots/
./schemes-benchmark.py
