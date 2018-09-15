#!/usr/bin/env python

import matplotlib.pyplot as plt
import os
import sys

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

plt.bar(ind, percent5, width, alpha=alpha, edgecolor="black", label='5%')
plt.bar(ind + width, percent10, width, alpha=alpha,
        edgecolor="black", label='10%')
plt.bar(ind + 2 * width, percent20, width, alpha=alpha,
        edgecolor="black", label='20%')
plt.bar(ind + 3 * width, percent50, width, alpha=alpha,
        edgecolor="black", label='50%')
plt.bar(ind + 4 * width, percent100, width, alpha=alpha,
        edgecolor="black", label='100%')

if value[0] == 'c':
    stage = "Construction"
else:
    stage = "Queries"

if "ios" in value:
    plt.ylabel("{0} IO requests".format(stage))
    plt.title("{0} stage. IO requests.".format(stage))
elif "vol" in value:
    plt.ylabel("{0} number of messages".format(stage))
    plt.title("{0} stage. Communication volume.".format(stage))
else:
    plt.ylabel("{0} messages' size".format(stage))
    plt.title("{0} stage. Communication size.".format(stage))

plt.xticks(ind + 2 * width, names, rotation=45)
plt.legend(loc='best')

plt.grid(linestyle='-', alpha=0.5)

fig = plt.figure(1)
fig.subplots_adjust(bottom=0.2)

if os.path.exists("results/protocol-data-percent-{0}.pdf".format(value)):
    os.remove("results/protocol-data-percent-{0}.pdf".format(value))

plt.savefig(
    "results/protocol-data-percent-{0}.pdf".format(value), format='pdf', dpi=1000)
