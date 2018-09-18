#!/usr/bin/env python

import matplotlib.pyplot as plt
import os
import matplotlib as mpl
import numpy as np

names = ('AES', 'PRG', 'PRF', 'Hash', 'PRP', 'HG Sampler')
N = len(names)

data = []

with open("./data/primitives-benchmark.txt") as fp:
    line = fp.readline()
    while line:
        data.append(int(line.strip()))
        line = fp.readline()

# data = ( 8100, 3541, 2993, 1063, 11696, 16751 )

ind = np.arange(N)
width = 0.35

plt.style.use('grayscale')

plt.bar(ind, data, width, alpha=0.5, edgecolor="black")

# plt.ylabel('Time (nanoseconds)')
# plt.title('Primitives benchmark')

ax = plt.gca()
ax.get_yaxis().set_major_formatter(
    mpl.ticker.FuncFormatter(lambda x, p: format(int(x), ','))
)

plt.xticks(ind, names, rotation=45)

plt.grid(linestyle='-', alpha=0.5)

fig = plt.figure(1)
fig.subplots_adjust(bottom=0.175)

if os.path.exists("results/primitives-benchmark.pdf"):
	os.remove("results/primitives-benchmark.pdf")

plt.savefig('results/primitives-benchmark.pdf', format='pdf', dpi=1000)
