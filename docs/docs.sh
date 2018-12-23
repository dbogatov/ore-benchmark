#!/usr/bin/env bash

set -e
shopt -s globstar

# Ensure that the CWD is set to script's location
cd "${0%/*}"
CWD=$(pwd)

SHA1="b26a800bd5c66268e22498eec35277b525b407b8"

rm -rf _site docfx-tmpl

git clone https://github.com/MathewSachin/docfx-tmpl.git
cd docfx-tmpl
git reset --hard $SHA1
cd ..

cp ../README.md index.md
docfx docfx.json --force

rm -rf ../src/web/wwwroot/documentation
cp -r _site ../src/web/wwwroot/documentation

echo "Done."
