# Simulations

## Different distributions

	# -d total data size
	# -q total query size
	# -s seed
	./tools/data-gen/generate.sh -d 2500 -q 100 -s 1305

	cd /tools/simulation/protocols

	# will run all protocols with given seed for all five distributions
	# elements per page are set in ./tools/simulation/protocols/protocols.sh
	# default cache is 128
	./parallel.sh -s 1306

	# will generate latex protocol table
	dotnet run -- --type Table --data ../../../results/protocols/ --tail 128-1306

	# will generate .txt files for plots (different data distributions)
	dotnet run -- --type Plots --data ../../../results/protocols/ --output ../../plots/data/ --tail 128-1306

	cd ./tools/plots

	# generate plots (./results/*.pdf)
	./plot-protocols.sh

## Different data percentages (scalability)

	# -d total data size
	# -q total query size
	# -s seed
	./tools/data-gen/generate.sh -d 2500 -q 100 -s 1405

	cd ./tools/simulation/protocols

	# will run all protocols with given seed for all data percentages
	./parallel-data-sizes.sh -s 1405 -c 15

	# will generate .txt files for plots (different data percentages)
	dotnet run -- --type DataPercent --data ../../../results/protocols/ --output ../../plots/data/ --tail 15-1405 --distro uniform

	cd ./tools/plots

	# generate plots (./results/*.pdf)
	./plot-protocols-data-percent.sh

## Different query sizes

	# -d total data size
	# -q total query size
	# -s seed
	./tools/data-gen/generate.sh -d 2500 -q 100 -s 1505

	cd ./tools/simulation/protocols

	# will run all protocols with given seed for all data percentages
	./parallel-query-sizes.sh -s 1505 -c 15

	# WARNING: currently one has to manually run Logarithmic-BRC with pack extension
	# and name the files coldpope-*json

	# will generate .txt files for plots (different query sizes)
	dotnet run -- --type QuerySizes --data ../../../results/protocols/ --output ../../plots/data/ --tail 15-1505 --distro uniform

	cd ./tools/plots

	# generate plots (./results/*.pdf)
	./plot-protocols-query-sizes.sh

## Cold vs warm (queries over time)

	# -d total data size
	# -q total query size
	# -s seed
	./tools/data-gen/generate.sh -d 2500 -q 100 -s 1605

	cd ./tools/simulation/protocols

	# will run all protocols with given seed with extended output
	./protocols.sh -s 1605 -c 15 -v

	cd ./tools/simulation/cold-vs-warm

	# will generate .txt files for plots
	dotnet run -- --data ../../../results/protocols/ --output ../../plots/data/ --tail uniform-.5-100-15-1605

	cd ./tools/plots

	# generate plots (./results/*.pdf)
	./plot-cold-vs-warm.sh
