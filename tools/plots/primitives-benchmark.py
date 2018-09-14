#!/usr/bin/env python

import matplotlib.pyplot as plt
import os

import numpy as np

names = ('AES', 'PRG', 'PRF', 'Hash', 'PRP', 'HG Sampler')
N = len(names)

data = []

with open("./data/primitives-benchmark.txt") as fp:
    line = fp.readline()
    while line:
        data.append(int(line.strip()))
        line = fp.readline()

data = ( 8100, 3541, 2993, 1063, 11696, 16751 )

ind = np.arange(N)
width = 0.35

plt.bar(ind, data, width, alpha=0.5, edgecolor="black")

plt.ylabel('Time (nanoseconds)')
plt.title('Primitives benchmark')

plt.xticks(ind, names, rotation=45)

fig = plt.figure(1)
fig.subplots_adjust(bottom=0.175)

if os.path.exists("results/primitives.pdf"):
	os.remove("results/primitives.pdf")

plt.savefig('results/primitives.pdf', format='pdf', dpi=1000)
