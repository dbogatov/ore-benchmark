#!/usr/bin/env bash

set -e
shopt -s globstar

# Ensure that the CWD is set to script's location
cd "${0%/*}"
CWD=$(pwd)

# See https://github.com/lucaslorentz/minicover

rm -rf ../../coverage*

dotnet restore ./../../test/
dotnet build ./../../test/

dotnet restore

# Instrument assemblies inside 'test' folder to detect hits for source files inside 'src' folder
dotnet minicover instrument --workdir ../../ --assemblies test/**/bin/**/*.dll --sources src/**/*.cs --sources test/**/*.cs

# Reset hits count in case minicover was run for this project
dotnet minicover reset

export ASPNETCORE_ENVIRONMENT="Testing"

dotnet test --no-build --no-restore --verbosity n ./../../test/ --logger trx --filter Category=Unit || true

# Uninstrument assemblies, it's important if you're going to publish or deploy build outputs
dotnet minicover uninstrument --workdir ../../

dotnet minicover report --workdir ../../ --threshold 90 | tee cov-report.txt
dotnet minicover htmlreport --workdir ../../ --threshold 90

alltotal=0
allhit=0

while IFS='' read -r line || [[ -n "$line" ]]
do
	if [[ $line == *"src/"* ]]
	then
		total=$(echo $line | cut -d"|" -f 3)
		hit=$(echo $line | cut -d"|" -f 4)
		# echo "$((hit)) of $((total))"
		alltotal=$((alltotal+total))
		allhit=$((allhit+hit))
	fi
done < cov-report.txt

coverage=$(bc -l <<< "scale=2; 100*$allhit/$alltotal")

echo "Final coverage is $coverage%"
