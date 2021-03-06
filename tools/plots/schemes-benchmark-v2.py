#!/usr/bin/env python3

import matplotlib.pyplot as plt
import os

import numpy as np

names = ('BCLO', 'CLWW', 'Lewi-Wu-16', 'Lewi-Wu-8', 'Lewi-Wu-4', 'FH-OPE')
N = len(names)

encryptions = []
comparisons = []

with open("./data/schemes-benchmark.txt") as fp:
    line = fp.readline()
    counter = 0
    while line:
        if counter < N:
            encryptions.append(float(line.strip()))
        else:
            comparisons.append(float(line.strip()))
        line = fp.readline()
        counter += 1

# encryptions = (29334.030, 892.111, 24254.990, 39712.080, 284570.568, 5.380)
# comparisons = (3.566, 3.477, 236.866, 209.566, 231.546, 4.135)

ind = np.arange(N)
width = 0.35

plt.style.use('grayscale')

plt.bar(ind, encryptions, width, alpha=0.5,
        edgecolor="black", label='Encryption')
plt.bar(ind + width, comparisons, width, alpha=0.5,
        edgecolor="black", label='Comparison')

plt.xticks(ind + width / 2, names, rotation=45)
plt.legend(loc='best')

plt.grid(linestyle='dotted', alpha=0.5)

ax = plt.gca()
ax.set_yscale("log", nonposy='clip')

legend = ax.legend(loc='upper left')
legend.get_frame().set_facecolor('#eaf0fb')

fig = plt.figure(1)
fig.subplots_adjust(bottom=0.175)

if os.path.exists("results/schemes-benchmark.pdf"):
	os.remove("results/schemes-benchmark.pdf")

plt.savefig('results/schemes-benchmark.pdf', format='pdf', dpi=1000, bbox_inches='tight', transparent=True)
