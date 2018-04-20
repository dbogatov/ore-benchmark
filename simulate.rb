#!/usr/bin/env ruby

build = "dotnet build -c release src/cli/"
puts ">>> #{build}"
puts `#{build}`

Run = Struct.new(:setsize, :querysize, :scheme, :type, :btreebranches, :cios, :cops, :ctime, :ccputime, :qios, :qops, :qtime, :qcputime)

runs = Array.new

for scheme in ["practicalore", "noencryption"] do
	for type in ["exact", "range", "update", "delete"] do
		for btreebranches in [2, 3, 5, 10, 20, 50] do
			
			cmd = "dotnet src/cli/bin/release/netcoreapp2.0/cli.dll --dataset data/dataset.txt --queries data/#{type}-queries.txt --queries-type #{type} --ore-scheme #{scheme} --b-plus-tree-branches #{btreebranches}"
			puts ">>> #{cmd}"
			output = `#{cmd}`;

			setsize = output.scan(/Dataset of (.*) records/)
			querysize = output.scan(/Queries of (.*) queries/)

			cios = output.scan(/Construction IOs: (.*)/)
			cops = output.scan(/Construction OPs: (.*)/)
			ctime = output.scan(/Construction Time: (.*)/)
			ccputime = output.scan(/Construction CPUTime: (.*)/)

			qios = output.scan(/Query IOs: (.*)/)
			qops = output.scan(/Query OPs: (.*)/)
			qtime = output.scan(/Query Time: (.*)/)
			qcputime = output.scan(/Query CPUTime: (.*)/)

			runs.push(Run.new(setsize, querysize, scheme, type, btreebranches, cios, cops, ctime, ccputime, qios, qops, qtime, qcputime))
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
		"Construction scheme OPs",
		"Construction observed time (ms)",
		"Construction CPU time (ms)",
		"Query IOs",
		"Query scheme OPs",
		"Query observed time (ms)",
		"Query CPU time (ms)"
	].join(",")
	runs.each { 
		|run| file.puts run.values.join(",")
	}
}
