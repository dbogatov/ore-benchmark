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

uniform = []
normal = []
zipf = []
employees = []
forest = []

with open("./data/protocols-{0}.txt".format(value)) as fp:
    line = fp.readline()
    counter = 0
    while line:
        if counter < N:
            if counter % N != 2 and counter % N != 4:
                uniform.append(int(line.strip()))
        elif counter < 2 * N:
            if counter % N != 2 and counter % N != 4:
                normal.append(int(line.strip()))
        elif counter < 3 * N:
            if counter % N != 2 and counter % N != 4:
                zipf.append(int(line.strip()))
        elif counter < 4 * N:
            if counter % N != 2 and counter % N != 4:
                employees.append(int(line.strip()))
        elif counter < 5 * N:
            if counter % N != 2 and counter % N != 4:
                forest.append(int(line.strip()))
        line = fp.readline()
        counter += 1

ind = np.arange(N - 2)
width = 1.0 / 6

alpha = 0.5

plt.style.use('grayscale')

if value != "qsize":
    f, (ax, ax2) = plt.subplots(2, 1, sharex=True)

    for axis in [ax, ax2]:
        axis.bar(ind, uniform, width, alpha=alpha, edgecolor="black", label='Uniform distribution')
        axis.bar(ind + width, normal, width, alpha=alpha,
                edgecolor="black", label='Normal distribution')
        # plt.bar(ind + 2 * width, zipf, width, alpha=alpha,
        #         edgecolor="black", label='Zipf')
        axis.bar(ind + 2 * width, employees, width, alpha=alpha,
                edgecolor="black", label='CA public employees dataset')
        # plt.bar(ind + 4 * width, forest, width, alpha=alpha,
        #         edgecolor="black", label='Forest Cover')

    if value == "cios":
        ax.set_ylim(481, 495)  # outliers only
        ax2.set_ylim(0, 11)  # most of the data
    elif value == "cvol":
        ax.set_ylim(37.5, 41)  # outliers only
        ax2.set_ylim(0, 5.5)  # most of the data
    elif value == "csize":
        ax.set_ylim(321, 700)  # outliers only
        ax2.set_ylim(0, 35.5)  # most of the data
    elif value == "qios":
        ax.set_ylim(32, 2200)  # outliers only
        ax2.set_ylim(0, 31.5)  # most of the data
    elif value == "qvol":
        ax.set_ylim(490000, 505000)  # outliers only
        ax2.set_ylim(0, 1020)  # most of the data

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

    legend = ax.legend(loc='upper left')
    legend.get_frame().set_facecolor('#eaf0fb')

    f.subplots_adjust(bottom=0.2)
else:
    plt.bar(ind, uniform, width, alpha=alpha, edgecolor="black", label='Uniform distribution')
    plt.bar(ind + width, normal, width, alpha=alpha,
            edgecolor="black", label='Normal distribution')
    # plt.bar(ind + 2 * width, zipf, width, alpha=alpha,
    #         edgecolor="black", label='Zipf')
    plt.bar(ind + 2 * width, employees, width, alpha=alpha,
            edgecolor="black", label='CA public employees dataset')
    # plt.bar(ind + 4 * width, forest, width, alpha=alpha,
    #         edgecolor="black", label='Forest Cover')

    legend = plt.legend(loc='best')
    legend.get_frame().set_facecolor('#eaf0fb')

    ax = plt.gca()
    ax.set_yscale("log", nonposy='clip')

    plt.grid(linestyle='dotted', alpha=0.5)

    fig = plt.figure(1)
    fig.subplots_adjust(bottom=0.2)

plt.xticks(ind + width, names, rotation=45)

if os.path.exists("results/protocol-charts-{0}.pdf".format(value)):
    os.remove("results/protocol-charts-{0}.pdf".format(value))

plt.savefig(
    "results/protocol-charts-{0}.pdf".format(value), format='pdf', dpi=1000, bbox_inches='tight', transparent=True)
