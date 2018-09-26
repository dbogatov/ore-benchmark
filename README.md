# ORE Schemes Simulator

## Abstract

Database operations over encrypted data has received a lot of attention recently due to increasing security concerns for many database applications.
One of the most promising approaches to perform database queries over encrypted data is to use specialized encryption schemes.
Order Preserving Encryption (OPE) and Order Revealing Encryption (ORE) schemes are examples of encryption schemes that trade security for performance and fit very well database systems approaches. However, until now, there is no clear comparison between the different schemes and protocols that have been proposed in this area.
In this paper, we present the first comprehensive comparison among a number of important OPE and ORE schemes using a new framework that we implemented from scratch.
We analyze and compare the schemes and protocols both theoretically and experimentally in a unified setting.
Our comparison reveals some interesting results concerning the relative security and performance of these schemes and protocols.
Furthermore, we propose a number of improvements for some of these schemes and provide a number of suggestions and recommendation that will be valuable to database system designers and practitioners.

## Analyzed schemes and protocols

- [Order Preserving Symmetric Encryption (aka **CryptDB OPE**)](https://eprint.iacr.org/2012/624.pdf)
- [Practical Order-Revealing Encryption with Limited Leakage (aka **Practical ORE**)](https://eprint.iacr.org/2015/1125.pdf)
- [Order-Revealing Encryption: New Constructions, Applications, and Lower Bounds (aka **Lewi ORE**)](https://eprint.iacr.org/2016/612.pdf)
- [Frequency-Hiding Order-Preserving Encryption (aka **FH-OPE**)](http://www.fkerschbaum.org/ccs15.pdf)
- [Reducing the Leakage in Practical Order-Revealing Encryption (aka **Adam ORE**)](https://eprint.iacr.org/2016/661.pdf)
- [An Efficiently Searchable Encrypted Data Structure for Range Queries (aka **Florian Protocol**)](https://arxiv.org/pdf/1709.09314.pdf)
- [POPE: Partial Order Preserving Encoding (aka (**POPE Protocol**)](https://arxiv.org/pdf/1610.04025.pdf)

We have generated synthetic (uniform and normal distributions) and real (CA public employees salaries) datasets.

The canonical project repository is [here](https://git.dbogatov.org/bu/ore-benchmark/Project-Code).

## Instructions

### To run the tool on your data (or our test data)

Either compile the code (see below), or use this docker image [dbogatov/ore-benchmark](https://hub.docker.com/r/dbogatov/ore-benchmark/).
Here are the few examples (for docker-based approach):

```bash
# note that you could simply start an interactive shell session by
docker run -it dbogatov/ore-benchmark

# to examine arguments and option for the tool
docker run dbogatov/ore-benchmark /bin/sh -c "dotnet ./cli.dll --help"
# or see help for specific commands
docker run dbogatov/ore-benchmark /bin/sh -c "dotnet ./cli.dll scheme --help"
docker run dbogatov/ore-benchmark /bin/sh -c "dotnet ./cli.dll protocol --help"

# to see our supplied data
docker run dbogatov/ore-benchmark /bin/sh -c "tree ./data"

# to run simple scheme simulation (e.g. CLWW) on supplied data set
docker run dbogatov/ore-benchmark /bin/sh -c "dotnet ./cli.dll --dataset ./data/uniform/data.txt -v --ore-scheme practicalore scheme"

# to run simple protocol simulation (e.g. POPE) on supplied data and query sets
docker run dbogatov/ore-benchmark /bin/sh -c "dotnet ./cli.dll --dataset ./data/uniform/data.txt -v --ore-scheme pope protocol --queries ./data/uniform/queries-1.txt"

# to see the format of data and query files
# data file line is an integer, coma, string (in quotes)
# query file line is two integers separated by coma
docker run dbogatov/ore-benchmark /bin/sh -c "head -n 10 ./data/uniform/data.txt"
docker run dbogatov/ore-benchmark /bin/sh -c "head -n 10 ./data/uniform/queries-1.txt"

# to run simulations on your data and queries
# assuming you have a directory /path/to/data/ and it contains data.txt and queries.txt
# here we have mapped your local directory into docker container
docker run \
	-v /path/to/data:/benchmark/your-data/ \
	dbogatov/ore-benchmark \
	/bin/sh -c "dotnet ./cli.dll --dataset ./your-data/data.txt -v --ore-scheme pope protocol --queries ./your-data/queries.txt"

# advanced; to generate JSON output and save it locally
# you have to have a directory /path/to/results, where result.json will appear
docker run \
	-v /path/to/data:/benchmark/your-data/ \
	-v /path/to/results:/benchmark/results/ \
	dbogatov/ore-benchmark \
	/bin/sh -c "dotnet ./cli.dll --dataset ./your-data/data.txt --ore-scheme pope protocol --queries ./your-data/queries.txt > ./results/result.json"
```

Running the tool locally without docker is more trivial (just omit all docker wrappers).

### To build the code locally

Prerequisites:

- .NET Core 2.1 (C# 7.3), or
- [This Docker image dbogatov/docker-sources:microsoft-dotnet-2.1-sdk-alpine](https://hub.docker.com/r/dbogatov/docker-sources/tags/)

```bash
# build with
dotnet build -c release ./src/cli/ -o dist/
# resulting binary is ./src/cli/dist/cli.dll

# run with
dotnet ./src/cli/dist/cli.dll --help
```

## Packages

- [B+ tree docs and repo](./src/b-plus-tree)
- [ORE/ OPE schemes docs and repo](./src/ore-schemes)
