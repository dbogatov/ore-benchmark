#!/usr/bin/env python

import matplotlib.pyplot as plt
import os
import sys
from brokenaxes import brokenaxes
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

# fig = plt.figure()
bax = brokenaxes(ylims=((0, 11), (225, 280)))


# bax.plot(x, np.sin(10 * x), label='sin')
# bax.plot(x, np.cos(10 * x), label='cos')
bax.legend(loc='best')
# bax.set_xlabel('time')
# bax.set_ylabel('value')


# for axis in [ax, ax2]:
bax.bar(ind, percent5, width, alpha=alpha, edgecolor="black", label='5%')
bax.bar(ind + width, percent10, width, alpha=alpha,
		edgecolor="black", label='10%')
bax.bar(ind + 2 * width, percent20, width, alpha=alpha,
		edgecolor="black", label='20%')
bax.bar(ind + 3 * width, percent50, width, alpha=alpha,
		edgecolor="black", label='50%')
bax.bar(ind + 4 * width, percent100, width, alpha=alpha,
		edgecolor="black", label='100%')

# ax.set_ylim(225, 280)  # outliers only
# ax2.set_ylim(0, 11)  # most of the data

# ax.spines['bottom'].set_visible(False)
# ax2.spines['top'].set_visible(False)
# ax.xaxis.tick_top()
# ax.tick_params(labeltop=False)  # don't put tick labels at the top
# ax2.xaxis.tick_bottom()

# d = .015  # how big to make the diagonal lines in axes coordinates
# # arguments to pass to plot, just so we don't keep repeating them
# kwargs = dict(transform=ax.transAxes, color='k', clip_on=False)
# ax.plot((-d, +d), (-d, +d), **kwargs)        # top-left diagonal
# ax.plot((1 - d, 1 + d), (-d, +d), **kwargs)  # top-right diagonal

# kwargs.update(transform=ax2.transAxes)  # switch to the bottom axes
# ax2.plot((-d, +d), (1 - d, 1 + d), **kwargs)  # bottom-left diagonal
# ax2.plot((1 - d, 1 + d), (1 - d, 1 + d), **kwargs)  # bottom-right diagonal

if value[0] == 'c':
    stage = "Construction"
else:
    stage = "Queries"

if "ios" in value:
    bax.set_xlabel("{0} IO requests".format(stage))
    bax.set_title("{0} stage. IO requests.".format(stage))
elif "vol" in value:
    bax.set_xlabel("{0} number of messages".format(stage))
    bax.set_title("{0} stage. Communication volume.".format(stage))
else:
    bax.set_xlabel("{0} messages' size".format(stage))
    bax.set_title("{0} stage. Communication size.".format(stage))

bax.axs[1].set_xticks(ind + 2 * width, names)
# plt.legend(loc='best')

# plt.grid(linestyle='-', alpha=0.5)
bax.minorticks_on()
bax.grid(axis='y', which='major', ls='-')
bax.grid(axis='y', which='minor', ls='--', alpha=0.5)


# fig = plt.figure(1)
# fig.subplots_adjust(bottom=0.2)

if os.path.exists("results/protocol-data-percent-{0}.pdf".format(value)):
    os.remove("results/protocol-data-percent-{0}.pdf".format(value))

plt.savefig(
    "results/protocol-data-percent-{0}.pdf".format(value), format='pdf', dpi=1000)
