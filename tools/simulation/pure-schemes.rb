#!/usr/bin/env ruby

require 'English'

Dir.chdir File.dirname(__FILE__)

def run(input, scheme, seed, lewioren, cryptdbrange)
  cmd = "dotnet ../../src/cli/dist/cli.dll --dataset ../../data/#{input}/data.txt --ore-scheme #{scheme} --seed #{seed} scheme --lewi-ore-n #{lewioren} --cryptdb-range #{cryptdbrange}"
  puts ">>> #{cmd}"
  output = `#{cmd}`

  open('../../results/schemes.json', 'a') do |f|
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

`rm -f ../../results/schemes.json`
`mkdir -p ../../results/`

`touch ../../results/schemes.json`

open('../../results/schemes.json', 'a') do |f|
  f << '{'
end

%w[cloz fhope lewiwu bclo clww noencryption].each do |scheme|
  case scheme

  when 'lewiwu'
    [16, 8, 4].each do |lewiwun|
      success = false unless run('uniform', scheme, prng.rand(2**30), lewiwun, 48)
    end

  when 'bclo'
    [32, 36, 40, 44, 48].each do |bclorange|
      success = false unless run('uniform', scheme, prng.rand(2**30), 16, bclorange)
    end

  else
    success = false unless run('uniform', scheme, prng.rand(2**30), 16, 48)
  end
end

open('../../results/schemes.json', 'a') do |f|
  f << '"" : {}}'
end

puts 'Results are in results/schemes.json in you current directory'

`rm -rf ../../src/**/dist`

exit(success)
