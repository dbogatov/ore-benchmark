#!/usr/bin/env ruby

build = "dotnet build -c release src/cli/"
puts ">>> #{build}"
puts `#{build}`

Run = Struct.new(:setsize, :querysize, :scheme, :type, :btreebranches, :cios, :avgcios, :cops, :avgcops, :ctime, :ccputime, :qios, :avgqios, :qops, :avgqops, :qtime, :qcputime)

runs = Array.new

for scheme in ["practicalore", "noencryption"] do
	for type in ["exact", "range-0.5", "range-1", "range-2", "range-3", "update", "delete"] do
		for btreebranches in [2, 5, 20, 50] do
			
			cmd = "dotnet src/cli/bin/release/netcoreapp2.0/cli.dll --dataset data/dataset.txt --queries data/#{type}-queries.txt --queries-type #{type.split(/-/).first} --ore-scheme #{scheme} --b-plus-tree-branches #{btreebranches}"
			puts ">>> #{cmd}"
			output = `#{cmd}`;

			setsize = output.scan(/Dataset of (.*) records/)
			querysize = output.scan(/Queries of (.*) queries/)

			cios = output.scan(/Construction IOs: (.*)/)
			avgcios = output.scan(/Construction AvgIOs: (.*)/)
			cops = output.scan(/Construction OPs: (.*)/)
			avgcops = output.scan(/Construction AvgOPs: (.*)/)
			ctime = output.scan(/Construction Time: (.*)/)
			ccputime = output.scan(/Construction CPUTime: (.*)/)

			qios = output.scan(/Query IOs: (.*)/)
			avgqios = output.scan(/Query AvgIOs: (.*)/)
			qops = output.scan(/Query OPs: (.*)/)
			avgqops = output.scan(/Query AvgOPs: (.*)/)
			qtime = output.scan(/Query Time: (.*)/)
			qcputime = output.scan(/Query CPUTime: (.*)/)

			runs.push(Run.new(setsize, querysize, scheme, type, btreebranches, cios, avgcios, cops, avgcops, ctime, ccputime, qios, avgqios, qops, avgqops, qtime, qcputime))
		end
	end
end

puts "Here are the results (also look for results.csv in you current directory)"

runs.each { |run| puts run.to_h }

`rm results.csv`

File.open("results.csv", 'w') { 
	|file| 
	file.puts [
		"Dataset size",
		"Query set size",
		"ORE scheme",
		"Query type",
		"B+ tree branches",
		"Construction IOs",
		"Construction AvgIOs",
		"Construction scheme OPs",
		"Construction scheme AvgOPs",
		"Construction observed time (ms)",
		"Construction CPU time (ms)",
		"Query IOs",
		"Query AvgIOs",
		"Query scheme OPs",
		"Query scheme AvgOPs",
		"Query observed time (ms)",
		"Query CPU time (ms)"
	].join(",")
	runs.each { 
		|run| file.puts run.values.join(",")
	}
}
