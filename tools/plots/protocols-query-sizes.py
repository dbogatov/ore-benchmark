#!/usr/bin/env python

import matplotlib.pyplot as plt
import os
import sys
from matplotlib.ticker import MaxNLocator
import matplotlib as mpl
import numpy as np

value = str(sys.argv[1])

names = ('No encryption', 'BCLO, CLWW,\nFH-OPE', 'Lewi-Wu', 'CLOZ',
         'Kerschbaum', 'POPE cold', 'POPE warm')
N = len(names) + 2

queries05 = []
queries10 = []
queries15 = []
queries20 = []
queries30 = []

with open("./data/protocols-query-sizes-{0}.txt".format(value)) as fp:
    line = fp.readline()
    counter = 0
    while line:
        if counter < N:
            if counter % N != 2 and counter % N != 4:
                queries05.append(int(line.strip()))
        elif counter < 2 * N:
            if counter % N != 2 and counter % N != 4:
                queries10.append(int(line.strip()))
        elif counter < 3 * N:
            if counter % N != 2 and counter % N != 4:
                queries15.append(int(line.strip()))
        elif counter < 4 * N:
            if counter % N != 2 and counter % N != 4:
                queries20.append(int(line.strip()))
        elif counter < 5 * N:
            if counter % N != 2 and counter % N != 4:
                queries30.append(int(line.strip()))
        line = fp.readline()
        counter += 1

ind = np.arange(N - 2)
width = 1.0 / 6

alpha = 0.5

plt.style.use('grayscale')

f, (ax, ax2) = plt.subplots(2, 1, sharex=True)

for axis in [ax, ax2]:
    axis.bar(ind, queries05, width, alpha=alpha, edgecolor="black", label='0.5%')
    axis.bar(ind + width, queries10, width, alpha=alpha,
            edgecolor="black", label='1%')
    axis.bar(ind + 2 * width, queries15, width, alpha=alpha,
            edgecolor="black", label='1.5%')
    axis.bar(ind + 3 * width, queries20, width, alpha=alpha,
            edgecolor="black", label='2%')
    axis.bar(ind + 4 * width, queries30, width, alpha=alpha,
            edgecolor="black", label='3%')

ax.set_ylim(100, 2200)  # outliers only
ax2.set_ylim(0, 45)  # most of the data

ax.spines['bottom'].set_visible(False)
ax2.spines['top'].set_visible(False)
ax.xaxis.tick_top()
ax.tick_params(labeltop=False)  # don't put tick labels at the top
ax2.xaxis.tick_bottom()

d = .015  # how big to make the diagonal lines in axes coordinates
# arguments to pass to plot, just so we don't keep repeating them
kwargs = dict(transform=ax.transAxes, color='k', clip_on=False)
ax.plot((-d, +d), (-d, +d), **kwargs)        # top-left diagonal
ax.plot((1 - d, 1 + d), (-d, +d), **kwargs)  # top-right diagonal

kwargs.update(transform=ax2.transAxes)  # switch to the bottom axes
ax2.plot((-d, +d), (1 - d, 1 + d), **kwargs)  # bottom-left diagonal
ax2.plot((1 - d, 1 + d), (1 - d, 1 + d), **kwargs)  # bottom-right diagonal

for axis in [ax, ax2]:
    axis.grid(linestyle='dotted', alpha=0.5)
    axis.yaxis.set_major_locator(MaxNLocator(integer=True))
    axis.get_yaxis().set_major_formatter(
        mpl.ticker.FuncFormatter(lambda x, p: format(int(x), ','))
    )

ax.legend(loc='upper left')

plt.xticks(ind + 2 * width, names, rotation=45)

f.subplots_adjust(bottom=0.2)

if os.path.exists("results/protocol-query-sizes-{0}.pdf".format(value)):
    os.remove("results/protocol-query-sizes-{0}.pdf".format(value))

plt.savefig(
    "results/protocol-query-sizes-{0}.pdf".format(value), format='pdf', dpi=1000, bbox_inches='tight')
