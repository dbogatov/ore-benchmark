# ORE Schemes Simulator

The paper is submitted to VLDB.
See [technical report on eprint](https://eprint.iacr.org/2018/953.pdf).

George Kollios and Dmytro Bogatov were supported by an NSF SaTC Frontier Award CNS-1414119.
Leonid Reyzin was supported in part by NSF grant 1422965.

## Abstract

Database operations over encrypted data has received a lot of attention recently due to increasing security concerns for many database applications.
One of the most promising approaches to perform database queries over encrypted data is to use specialized encryption schemes.
Order Preserving Encryption (OPE) and Order Revealing Encryption (ORE) schemes are examples of encryption schemes that trade security for performance and fit very well database systems approaches. However, until now, there is no clear comparison between the different schemes and protocols that have been proposed in this area.
In this paper, we present the first comprehensive comparison among a number of important OPE and ORE schemes using a new framework that we implemented from scratch.
We analyze and compare the schemes and protocols both theoretically and experimentally in a unified setting.
Our comparison reveals some interesting results concerning the relative security and performance of these schemes and protocols.
Furthermore, we propose a number of improvements for some of these schemes and provide a number of suggestions and recommendation that will be valuable to database system designers and practitioners.

## Analyzed schemes and protocols

- [Order Preserving Symmetric Encryption (aka **BCLO OPE**)](https://eprint.iacr.org/2012/624.pdf)
- [Practical Order-Revealing Encryption with Limited Leakage (aka **CLWW ORE**)](https://eprint.iacr.org/2015/1125.pdf)
- [Order-Revealing Encryption: New Constructions, Applications, and Lower Bounds (aka **Lewi-Wu ORE**)](https://eprint.iacr.org/2016/612.pdf)
- [Frequency-Hiding Order-Preserving Encryption (aka **FH-OPE**)](http://www.fkerschbaum.org/ccs15.pdf)
- [Reducing the Leakage in Practical Order-Revealing Encryption (aka **CLOZ ORE**)](https://eprint.iacr.org/2016/661.pdf)
- [POPE: Partial Order Preserving Encoding (aka **POPE Protocol**)](https://arxiv.org/pdf/1610.04025.pdf)
- [Practical Private Range Search Revisited](http://www.idemertzis.com/Papers/sigmod16.pdf) working on top of
	- [Highly-Scalable Searchable Symmetric Encryption with Support for Boolean Queries (aka CJJKRS)](https://eprint.iacr.org/2013/169.pdf)
	- [Dynamic Searchable Encryption in Very-Large Databases: Data Structures and Implementation (aka CJJJKRS)](https://eprint.iacr.org/2014/853.pdf)

We have generated synthetic (uniform and normal distributions) and real (CA public employees salaries) data sets.

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
docker run dbogatov/ore-benchmark /bin/sh -c "dotnet ./cli.dll --dataset ./data/uniform/data.txt -v --protocol clww scheme"

# to run simple protocol simulation (e.g. POPE) on supplied data and query sets
docker run dbogatov/ore-benchmark /bin/sh -c "dotnet ./cli.dll --dataset ./data/uniform/data.txt -v --protocol pope protocol --queries ./data/uniform/queries-1.txt"

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
	/bin/sh -c "dotnet ./cli.dll --dataset ./your-data/data.txt -v --protocol pope protocol --queries ./your-data/queries.txt"

# advanced; to generate JSON output and save it locally
# you have to have a directory /path/to/results, where result.json will appear
docker run \
	-v /path/to/data:/benchmark/your-data/ \
	-v /path/to/results:/benchmark/results/ \
	dbogatov/ore-benchmark \
	/bin/sh -c "dotnet ./cli.dll --dataset ./your-data/data.txt --protocol pope protocol --queries ./your-data/queries.txt > ./results/result.json"
```

Running the tool locally without docker is more trivial (just omit all docker wrappers).

### To build the code locally

Prerequisites:

- .NET Core 2.2 (C# 7.3), or
- [This Docker image dbogatov/docker-sources:microsoft-dotnet-2.2-sdk-alpine](https://hub.docker.com/r/dbogatov/docker-sources/tags/)

```bash
# build with
dotnet build -c release ./src/cli/ -o dist/
# resulting binary is ./src/cli/dist/cli.dll

# run with
dotnet ./src/cli/dist/cli.dll --help
```

## Packages

- [B+ tree docs and code](https://git.dbogatov.org/bu/ore-benchmark/Project-Code/tree/master/src/b-plus-tree)
- [ORE / OPE / SSE schemes and primitives docs and code](https://git.dbogatov.org/bu/ore-benchmark/Project-Code/tree/master/src/crypto)
