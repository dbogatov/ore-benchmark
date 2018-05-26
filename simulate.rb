#!/usr/bin/env ruby

seed = ARGV.count == 1 ? ARGV[0].to_i : Random.new.rand(2**30)
prng = Random.new(seed)

puts "Global seed to be used: #{seed}"

build = 'dotnet build -c release src/cli/'
puts ">>> #{build}"
puts `#{build}`

Run = Struct.new(:setsize, :querysize, :scheme, :type, :btreebranches, :ccache, :cios, :avgcios, :cops, :avgcops, :ctime, :ccputime, :qcache, :qios, :avgqios, :qops, :avgqops, :qtime, :qcputime)

runs = []

for scheme in %w[cryptdb practicalore noencryption] do
  for type in ['exact', 'range-0.5', 'range-1', 'range-2', 'range-3', 'update', 'delete'] do
    for btreebranches in [2, 5, 20, 50] do
      for cache in [0, 10, 100] do

        cmd = "dotnet src/cli/bin/release/netcoreapp2.0/cli.dll --dataset data/dataset.txt --queries data/#{type}-queries.txt --queries-type #{type.split(/-/).first} --ore-scheme #{scheme} --b-plus-tree-branches #{btreebranches} --cache-size #{cache} --seed #{prng.rand(2**30)}"
        puts ">>> #{cmd}"
        output = `#{cmd}`

        setsize = output.scan(/Dataset of (.*) records/)
        querysize = output.scan(/Queries of (.*) queries/)

        ccache = output.scan(/Construction CacheSize: (.*)/)
        cios = output.scan(/Construction IOs: (.*)/)
        avgcios = output.scan(/Construction AvgIOs: (.*)/)
        cops = output.scan(/Construction OPs: (.*)/)
        avgcops = output.scan(/Construction AvgOPs: (.*)/)
        ctime = output.scan(/Construction Time: (.*)/)
        ccputime = output.scan(/Construction CPUTime: (.*)/)

        qcache = output.scan(/Query CacheSize: (.*)/)
        qios = output.scan(/Query IOs: (.*)/)
        avgqios = output.scan(/Query AvgIOs: (.*)/)
        qops = output.scan(/Query OPs: (.*)/)
        avgqops = output.scan(/Query AvgOPs: (.*)/)
        qtime = output.scan(/Query Time: (.*)/)
        qcputime = output.scan(/Query CPUTime: (.*)/)

        runs.push(Run.new(setsize, querysize, scheme, type, btreebranches, ccache, cios, avgcios, cops, avgcops, ctime, ccputime, qcache, qios, avgqios, qops, avgqops, qtime, qcputime))
      end
    end
  end
end

puts 'Here are the results (also look for results.csv in you current directory)'

runs.each { |run| puts run.to_h }

`rm results.csv`

File.open('results.csv', 'w') do |file|
  file.puts [
    'Dataset size',
    'Query set size',
    'ORE scheme',
    'Query type',
    'B+ tree branches',
    'Construction Cache Size',
    'Construction IOs',
    'Construction AvgIOs',
    'Construction scheme OPs',
    'Construction scheme AvgOPs',
    'Construction observed time (ms)',
    'Construction CPU time (ms)',
    'Query Cache Size',
    'Query IOs',
    'Query AvgIOs',
    'Query scheme OPs',
    'Query scheme AvgOPs',
    'Query observed time (ms)',
    'Query CPU time (ms)'
  ].join(',')
  runs.each do |run|
    file.puts run.values.join(',')
  end
end
