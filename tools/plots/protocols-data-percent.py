#!/usr/bin/env python

import matplotlib.pyplot as plt
import os
import sys
from matplotlib.ticker import MaxNLocator
import matplotlib as mpl
import numpy as np

value = str(sys.argv[1])

names = ('No encryption', 'BCLO', 'CLWW', 'Lewi-Wu', 'FH-OPE', 'CLOZ',
         'Kerschbaum', 'POPE cold', 'POPE warm')
N = len(names)

percent5 = []
percent10 = []
percent20 = []
percent50 = []
percent100 = []

with open("./data/protocols-data-percent-{0}.txt".format(value)) as fp:
    line = fp.readline()
    counter = 0
    while line:
        if counter < N:
            percent5.append(int(line.strip()))
        elif counter < 2 * N:
            percent10.append(int(line.strip()))
        elif counter < 3 * N:
            percent20.append(int(line.strip()))
        elif counter < 4 * N:
            percent50.append(int(line.strip()))
        elif counter < 5 * N:
            percent100.append(int(line.strip()))
        line = fp.readline()
        counter += 1

ind = np.arange(N)
width = 1.0 / 6

alpha = 0.5

plt.style.use('grayscale')

if value != "cios":
    f, (ax, ax2) = plt.subplots(2, 1, sharex=True)

    for axis in [ax, ax2]:
        axis.bar(ind, percent5, width, alpha=alpha, edgecolor="black", label='5%')
        axis.bar(ind + width, percent10, width, alpha=alpha,
                edgecolor="black", label='10%')
        axis.bar(ind + 2 * width, percent20, width, alpha=alpha,
                edgecolor="black", label='20%')
        axis.bar(ind + 3 * width, percent50, width, alpha=alpha,
                edgecolor="black", label='50%')
        axis.bar(ind + 4 * width, percent100, width, alpha=alpha,
                edgecolor="black", label='100%')

    if value == "cvol":
        ax.set_ylim(22, 35)  # outliers only
        ax2.set_ylim(0, 5.5)  # most of the data
    elif value == "csize":
        ax.set_ylim(315, 570)  # outliers only
        ax2.set_ylim(0, 55)  # most of the data
    elif value == "qios":
        ax.set_ylim(255, 370)  # outliers only
        ax2.set_ylim(0, 105)  # most of the data
    elif value == "qvol":
        ax.set_ylim(1050, 60000)  # outliers only
        ax2.set_ylim(0, 760)  # most of the data
    elif value == "qsize":
        ax.set_ylim(40001, 1000000)  # outliers only
        ax2.set_ylim(0, 40050)  # most of the data


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

    plt.xticks(ind + 2 * width, names, rotation=45)

    for axis in [ax, ax2]:
        axis.grid(linestyle='dotted', alpha=0.5)
        axis.yaxis.set_major_locator(MaxNLocator(integer=True))
        axis.get_yaxis().set_major_formatter(
            mpl.ticker.FuncFormatter(lambda x, p: format(int(x), ','))
        )

    ax.legend(loc='upper left')

    f.subplots_adjust(bottom=0.2)
else:
    plt.bar(ind, percent5, width, alpha=alpha, edgecolor="black", label='5%')
    plt.bar(ind + width, percent10, width, alpha=alpha,
            edgecolor="black", label='10%')
    plt.bar(ind + 2 * width, percent20, width, alpha=alpha,
            edgecolor="black", label='20%')
    plt.bar(ind + 3 * width, percent50, width, alpha=alpha,
            edgecolor="black", label='50%')
    plt.bar(ind + 4 * width, percent100, width, alpha=alpha,
            edgecolor="black", label='100%')

    plt.legend(loc='best')

    plt.grid(linestyle='dotted', alpha=0.5)

    fig = plt.figure(1)
    fig.subplots_adjust(bottom=0.2)

plt.xticks(ind + 2 * width, names, rotation=45)

if os.path.exists("results/protocol-data-percent-{0}.pdf".format(value)):
    os.remove("results/protocol-data-percent-{0}.pdf".format(value))

plt.savefig(
    "results/protocol-data-percent-{0}.pdf".format(value), format='pdf', dpi=1000, bbox_inches='tight')
