#!/usr/bin/env python

import matplotlib.pyplot as plt
import os
import matplotlib as mpl
import numpy as np
from matplotlib.ticker import MaxNLocator

names = ('AES', 'PRG', 'PRF', 'Hash', 'PRP', 'HG Sampler')
N = len(names)

data = []

with open("./data/primitives-benchmark.txt") as fp:
    line = fp.readline()
    while line:
        data.append(float(line.strip()))
        line = fp.readline()

# data = ( 8100, 3541, 2993, 1063, 11696, 16751 )

ind = np.arange(N)
width = 0.35

plt.style.use('grayscale')

plt.bar(ind, data, width, alpha=0.5, edgecolor="black")

ax = plt.gca()
ax.yaxis.set_major_locator(MaxNLocator(integer=True))

plt.xticks(ind, names, rotation=45)

plt.grid(linestyle='dotted', alpha=0.5)

fig = plt.figure(1)
fig.subplots_adjust(bottom=0.175)

if os.path.exists("results/primitives-benchmark.pdf"):
	os.remove("results/primitives-benchmark.pdf")

plt.savefig('results/primitives-benchmark.pdf', format='pdf', dpi=1000, bbox_inches='tight', transparent=True)
