#!/usr/bin/env ruby

require 'English'

Dir.chdir File.dirname(__FILE__)

def run(input, scheme, seed, lewioren, cryptdbrange)
  cmd = "dotnet ../../src/cli/dist/cli.dll --dataset ../../data/#{input}.txt --ore-scheme #{scheme} --seed #{seed} scheme --lewi-ore-n #{lewioren} --cryptdb-range #{cryptdbrange}"
  puts ">>> #{cmd}"
  output = `#{cmd}`

  open('../../results/schemes.csv', 'a') do |f|
    f << output
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

`rm -f ../../results/schemes.csv`
`mkdir -p ../../results/`

`touch ../../results/schemes.csv`

names = []

names.push('Seed')
names.push('LewiORE N')
names.push('CryptDB Range')
names.push('Scheme')

%w[Encryption Decryption Comparison].each do |operation|
  names.push("#{operation} operations number")

  %w[PRF PRG Hash LFPRF PRP HGSampler UniformSampler BinomialSampler PPH].each do |primitive|
    %w[impure pure].each do |purity|
      %w[total average].each do |number|
        names.push("#{operation} #{primitive} usage #{number} (#{purity})")
      end
    end
  end

  names.push("#{operation} observable time (ms)")
  names.push("#{operation} CPU time (ms)")
end

open('../../results/schemes.csv', 'a') do |f|
  f << names.join(',')
  f << "\n"
end

%w[fhope lewiore cryptdb practicalore noencryption].each do |scheme|
  case scheme

  when 'lewiore'
    [16, 8, 4].each do |lewioren|
      success = false unless run('schemes-dataset', scheme, prng.rand(2**30), lewioren, 48)
    end

  when 'cryptdb'
    [32, 36, 40, 44, 48].each do |cryptdbrange|
      success = false unless run('schemes-dataset', scheme, prng.rand(2**30), 16, cryptdbrange)
    end

  else
    success = false unless run('schemes-dataset', scheme, prng.rand(2**30), 16, 48)
  end
end

puts 'Results are in results/schemes.csv in you current directory'

`rm -rf ../../src/**/dist`

exit(success)
