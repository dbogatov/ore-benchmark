#!/usr/bin/env ruby

require 'English'

Dir.chdir File.dirname(__FILE__)

def run(input, queries, scheme, seed, cache, branching)
  cmd = "dotnet ../../src/cli/dist/cli.dll --dataset ../../data/#{input}/data.txt --ore-scheme #{scheme} --seed #{seed} protocol --queries ../../data/#{input}/#{queries}.txt --cache-size #{cache} --b-plus-tree-branches #{branching}"
  puts ">>> #{cmd}"
  output = `#{cmd}`

  open('../../results/protocol.json', 'a') do |f|
    f << "\"#{cmd}\": #{output},"
  end

  $CHILD_STATUS.success?
end

seed = ARGV.count == 1 ? ARGV[0].to_i : Random.new.rand(2**30)
prng = Random.new(seed)

puts "Global seed to be used: #{seed}"

build = 'dotnet build -c release ../../src/cli/ -o dist/'
puts ">>> #{build}"
puts `#{build}`

success = true

`rm -f ../../results/protocol.json`
`mkdir -p ../../results/`

`touch ../../results/protocol.json`

open('../../results/protocol.json', 'a') do |f|
  f << '{'
end

%w[pope kerschbaum cloz fhope lewiwu bclo clww noencryption cjjkrs cjjjkrs oram].each do |scheme|
  ['queries-.5', 'queries-3'].each do |queries|
    [5, 50].each do |btreebranches|
      success = false unless run('uniform', queries, scheme, prng.rand(2**30), 10, btreebranches)
    end
  end
end

open('../../results/protocol.json', 'a') do |f|
  f << '"" : {}}'
end

puts 'Results are in results/protocol.json in you current directory'

`rm -rf ../../src/**/dist`

exit(success)
