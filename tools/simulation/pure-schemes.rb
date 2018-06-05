#!/usr/bin/env ruby

require 'English'

Dir.chdir File.dirname(__FILE__)

seed = ARGV.count == 1 ? ARGV[0].to_i : Random.new.rand(2**30)
prng = Random.new(seed)

puts "Global seed to be used: #{seed}"

build = 'dotnet build -c release ../../src/cli/ -o dist/'
puts ">>> #{build}"
puts `#{build}`

Run = Struct.new(:setsize, :scheme, :qops, :avgqops, :qtime, :qcputime)

runs = []

success = true

%w[lewiore cryptdb practicalore noencryption].each do |scheme|
  cmd = "dotnet ../../src/cli/dist/cli.dll --dataset ../../data/exact-queries.txt --ore-scheme #{scheme} --seed #{prng.rand(2**30)} scheme"
  puts ">>> #{cmd}"
  output = `#{cmd}`

  success = false unless $CHILD_STATUS.success?

  setsize = output.scan(/Dataset of (.*) records/)

  ops = output.scan(/OPs: (.*)/)
  avgops = output.scan(/AvgOPs: (.*)/)
  time = output.scan(/Time: (.*)/)
  cputime = output.scan(/CPUTime: (.*)/)

  runs.push(Run.new(setsize, scheme, ops, avgops, time, cputime))
end

`rm -f ../../results/schemes.csv`
`mkdir -p ../../results/`

File.open('../../results/schemes.csv', 'w') do |file|
  file.puts [
    'Dataset size',
    'ORE scheme',
    'Scheme OPs',
    'Scheme AvgOPs',
    'Observed time (ms)',
    'CPU time (ms)'
  ].join(',')
  runs.each do |run|
    file.puts run.values.join(',')
  end
end

puts 'Results are in results/tree.csv in you current directory'

`rm -rf ../../src/**/dist`

exit(success)
