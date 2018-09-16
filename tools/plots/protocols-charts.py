#!/usr/bin/env python

import matplotlib.pyplot as plt
import os
import sys

import numpy as np

value = str(sys.argv[1])

names = ('No encryption', 'BCLO', 'CLWW', 'Lewi-Wu', 'FH-OPE', 'CLOZ',
         'Kerschbaum', 'POPE cold', 'POPE warm')
N = len(names)

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
            uniform.append(int(line.strip()))
        elif counter < 2 * N:
            normal.append(int(line.strip()))
        elif counter < 3 * N:
            zipf.append(int(line.strip()))
        elif counter < 4 * N:
            employees.append(int(line.strip()))
        elif counter < 5 * N:
            forest.append(int(line.strip()))
        line = fp.readline()
        counter += 1

ind = np.arange(N)
width = 1.0 / 6

alpha = 0.5

plt.bar(ind, uniform, width, alpha=alpha, edgecolor="black", label='Uniform')
plt.bar(ind + width, normal, width, alpha=alpha,
        edgecolor="black", label='Normal')
plt.bar(ind + 2 * width, zipf, width, alpha=alpha,
        edgecolor="black", label='Zipf')
plt.bar(ind + 3 * width, employees, width, alpha=alpha,
        edgecolor="black", label='CA employees')
plt.bar(ind + 4 * width, forest, width, alpha=alpha,
        edgecolor="black", label='Forest Cover')

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

ax = plt.gca()
ax.set_yscale("log", nonposy='clip')

plt.grid(linestyle='-', alpha=0.5)

fig = plt.figure(1)
fig.subplots_adjust(bottom=0.2)

if os.path.exists("results/protocol-charts-{0}.pdf".format(value)):
    os.remove("results/protocol-charts-{0}.pdf".format(value))

plt.savefig(
    "results/protocol-charts-{0}.pdf".format(value), format='pdf', dpi=1000)
