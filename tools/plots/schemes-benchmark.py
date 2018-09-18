#!/usr/bin/env python

import os
import numpy as np
import matplotlib.pyplot as plt

width = 1
groupgap = 1.5

bcloY = []
clwwY = []
lewiY = []
fhopeY = []

with open("./data/schemes-benchmark.txt") as fp:
    line = fp.readline()
    counter = 0
    while line:
        if counter == 0:
            bcloY.append(int(line.strip()))
        elif counter == 1:
            clwwY.append(int(line.strip()))
        elif counter < 5:
			lewiY.append(int(line.strip()))
        elif counter == 5:
			fhopeY.append(int(line.strip()))
        else:
			break
        line = fp.readline()
        counter += 1

# bcloY = [29334.030]
# clwwY = [892.111]
# lewiY = [24254.990, 39712.080, 284570.568]
# fhopeY = [5.380]

bcloX = np.arange(len(bcloY))
clwwX = np.arange(len(clwwY)) + groupgap + len(bcloY)
lewiX = np.arange(len(lewiY)) + groupgap + len(clwwY) + groupgap + len(bcloY)
fhopeX = np.arange(len(fhopeY)) + groupgap + len(lewiY) + \
    groupgap + len(clwwY) + groupgap + len(bcloY)

ind = np.concatenate((bcloX, clwwX, lewiX, fhopeX))
fig, ax = plt.subplots()

bcloRects = ax.bar(bcloX, bcloY, width, alpha=0.5,
                   edgecolor="black", label="BCLO")
clwwRects = ax.bar(clwwX, clwwY, width, alpha=0.5,
                   edgecolor="black", label="CLWW")
lewiRects = ax.bar(lewiX, lewiY, width, alpha=0.5,
                   edgecolor="black", label="Lewi-Wu")
fhopeRects = ax.bar(fhopeX, fhopeY, width, alpha=0.5,
                    edgecolor="black", label="FH-OPE")

ax.set_xticks(ind)
ax.set_xticklabels(('', '', '16', '8', '4', ''), fontsize=12)

ax.set_yscale("log", nonposy='clip')
plt.grid(linestyle='-', alpha=0.5)

plt.legend(['BCLO', 'CLWW', 'Lewi-Wu', 'FH-OPE'], loc='upper right')

if os.path.exists("results/schemes-benchmark.pdf"):
	os.remove("results/schemes-benchmark.pdf")

plt.savefig('results/schemes-benchmark.pdf', format='pdf', dpi=1000)
