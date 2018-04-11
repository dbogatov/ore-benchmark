# Instructions

Run with

	dotnet run --project src/cli/ -- --dataset data/dataset.txt --queries data/exact-queries.txt
	dotnet run --project src/cli/ -- --dataset data/dataset.txt --queries data/range-queries.txt --queries-type range
	dotnet run --project src/cli/ -- --dataset data/dataset.txt --queries data/update-queries.txt --queries-type update
	dotnet run --project src/cli/ -- --dataset data/dataset.txt --queries data/delete-queries.txt --queries-type delete

# Intermediate project report

## Experimental evaluation of ORE schemes for use in encrypted databases

### Abstract

The goal of this project is to provide an overview of the pros and cons of various order revealing encryption (ORE) schemes.
By simulating a real database using a B+ tree we examine how ORE schemes make efficiency differ for various queries.
We will be using two encryption schemes from the following papers: [Order Preserving Symmetric Encryption](https://eprint.iacr.org/2012/624.pdf), and [Practical Order-Revealing Encryption with Limited Leakage](https://eprint.iacr.org/2015/1125.pdf). 
We will be using benchmark data from the following website: [tpc.org](http://www.tpc.org) for evaluation.

The original project repository is [here](https://git.dbogatov.org/bu/CS-562/Project-Code).

### Progress

* Implemented
	- B+ tree 
		* Insert
		* Delete
		* Update
		* Exact match query
		* Range query
	- CryptDB OPE scheme (mostly implemented)
	- Practical ORE scheme (half done)
	- Simulator
		* Read dataset and queries from file
		* Run simulation on B+ tree
		* Track user and CPU time, number of I/O and scheme ops
* To be done
	- CryptDB OPE scheme
	- Practical ORE scheme
	- Generating benchmark data
	- Running simulation on large data sets
	- Writing up project paper
