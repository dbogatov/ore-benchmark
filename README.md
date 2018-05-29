# ORE Schemes Simulator

The goal of this project is to provide an overview of the pros and cons of various order revealing encryption (ORE) schemes.
By simulating a real database using a B+ tree we examine how ORE schemes make efficiency differ for various queries.
We will be using multiple encryption schemes from the following papers:
- [Order Preserving Symmetric Encryption](https://eprint.iacr.org/2012/624.pdf)
- [Practical Order-Revealing Encryption with Limited Leakage](https://eprint.iacr.org/2015/1125.pdf)
- [Order-Revealing Encryption: New Constructions, Applications, and Lower Bounds](https://eprint.iacr.org/2016/612.pdf)
- to be more...

We will be using benchmark data from the following website: [tpc.org](http://www.tpc.org) for evaluation.

The original project repository is [here](https://git.dbogatov.org/bu/CS-562/Project-Code).

## Instructions

Run with

	dotnet build -c release src/cli/

	dotnet src/cli/bin/release/netcoreapp2.0/cli.dll --dataset data/dataset.txt --queries data/exact-queries.txt -v
	dotnet src/cli/bin/release/netcoreapp2.0/cli.dll --dataset data/dataset.txt --queries data/range-0.5-queries.txt --queries-type range -v
	dotnet src/cli/bin/release/netcoreapp2.0/cli.dll --dataset data/dataset.txt --queries data/range-1-queries.txt --queries-type range -v
	dotnet src/cli/bin/release/netcoreapp2.0/cli.dll --dataset data/dataset.txt --queries data/range-2-queries.txt --queries-type range -v
	dotnet src/cli/bin/release/netcoreapp2.0/cli.dll --dataset data/dataset.txt --queries data/range-3-queries.txt --queries-type range -v
	dotnet src/cli/bin/release/netcoreapp2.0/cli.dll --dataset data/dataset.txt --queries data/update-queries.txt --queries-type update -v
	dotnet src/cli/bin/release/netcoreapp2.0/cli.dll --dataset data/dataset.txt --queries data/delete-queries.txt --queries-type delete -v
