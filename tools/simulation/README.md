# Simulations

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
