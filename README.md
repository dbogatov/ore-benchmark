## Instructions

Run with

	dotnet run --project src/cli/ -- --dataset data/dataset.txt --queries data/exact-queries.txt
	dotnet run --project src/cli/ -- --dataset data/dataset.txt --queries data/range-queries.txt --queries-type range
	dotnet run --project src/cli/ -- --dataset data/dataset.txt --queries data/update-queries.txt --queries-type update
	dotnet run --project src/cli/ -- --dataset data/dataset.txt --queries data/delete-queries.txt --queries-type delete
