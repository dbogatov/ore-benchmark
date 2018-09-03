# ORE Schemes Simulator

The goal of this project is to provide an overview of the pros and cons of various order revealing encryption (ORE) schemes.
By simulating a real client-server range queries protocols using a B+ tree we examine how ORE schemes make efficiency differ for various queries.
We will be using multiple encryption schemes and protocols from the following papers:

- [Order Preserving Symmetric Encryption (aka CryptDB OPE)](https://eprint.iacr.org/2012/624.pdf)
- [Practical Order-Revealing Encryption with Limited Leakage (aka Practical ORE)](https://eprint.iacr.org/2015/1125.pdf)
- [Order-Revealing Encryption: New Constructions, Applications, and Lower Bounds (aka Lewi ORE)](https://eprint.iacr.org/2016/612.pdf)
- [Frequency-Hiding Order-Preserving Encryption (aka FH-OPE)](http://www.fkerschbaum.org/ccs15.pdf)
- [Reducing the Leakage in Practical Order-Revealing Encryption (aka Adam ORE)](https://eprint.iacr.org/2016/661.pdf)
- [An Efficiently Searchable Encrypted Data Structure for Range Queries (aka Florian Protocol)](https://arxiv.org/pdf/1709.09314.pdf)
- [POPE: Partial Order Preserving Encoding (aka POPE Protocol)](https://arxiv.org/pdf/1610.04025.pdf)

We will be using benchmark data from the following website: [tpc.org](http://www.tpc.org) for evaluation.

The canonical project repository is [here](https://git.dbogatov.org/bu/CS-562/Project-Code).

## Instructions

Prerequisites:

- .NET Core 2.1 (C# 7.3)
- Ruby runtime
- Bash 4
- [This Docker image dbogatov/docker-sources:microsoft-dotnet-2.1-sdk-alpine](https://hub.docker.com/r/dbogatov/docker-sources/tags/) and this command `apk add --update bash ruby ruby-bundler` would work

Run with

	# generate data sets and query sets
	./tools/data-gen/generate.sh -d 20 -q 20 -s 1305

	# run pure schemes simulation
	./tools/simulation/pure-schemes.rb

	# run protocols simulation
	./tools/simulation/protocol.rb

	# run this command to see help message
	dotnet ../../src/cli/dist/cli.dll --help

## Packages

- [B+ tree docs and repo](./src/b-plus-tree)
