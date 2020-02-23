#!/usr/bin/env bash

set -e
shopt -s globstar

cd /benchmark

if [ -z "$CI_BUILD_REF" ]
then
	CI_BUILD_REF="local-dev"
fi

printf "namespace CLI { public partial class Version { public override string ToString() => \"%s\"; } }" $(echo $CI_BUILD_REF | cut -c1-8) > ./src/cli/Version.cs

dotnet restore src/cli/ --disable-parallel
dotnet restore src/benchmark/ --disable-parallel
dotnet restore tools/data-gen/ --disable-parallel

dotnet publish -c release src/cli/ -o dist/
dotnet build -c release src/benchmark/
dotnet build -c release tools/data-gen/

./tools/data-gen/generate.sh -d 2500 -q 50 -s 1305

mkdir -p ./scripts/
cp ./tools/reproducibility/{benchmarks,simulations,generate}.sh ./scripts/
