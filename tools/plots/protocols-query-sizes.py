#!/usr/bin/env python

import matplotlib.pyplot as plt
import os
import sys

import numpy as np

value = str(sys.argv[1])

names = ('No encryption', 'BCLO', 'CLWW', 'Lewi-Wu', 'FH-OPE', 'CLOZ',
         'Kerschbaum', 'POPE cold', 'POPE warm')
N = len(names)

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
            queries05.append(int(line.strip()))
        elif counter < 2 * N:
            queries10.append(int(line.strip()))
        elif counter < 3 * N:
            queries15.append(int(line.strip()))
        elif counter < 4 * N:
            queries20.append(int(line.strip()))
        elif counter < 5 * N:
            queries30.append(int(line.strip()))
        line = fp.readline()
        counter += 1

ind = np.arange(N)
width = 1.0 / 6

alpha = 0.5

plt.bar(ind, queries05, width, alpha=alpha, edgecolor="black", label='0.5%')
plt.bar(ind + width, queries10, width, alpha=alpha,
        edgecolor="black", label='1%')
plt.bar(ind + 2 * width, queries15, width, alpha=alpha,
        edgecolor="black", label='1.5%')
plt.bar(ind + 3 * width, queries20, width, alpha=alpha,
        edgecolor="black", label='2%')
plt.bar(ind + 4 * width, queries30, width, alpha=alpha,
        edgecolor="black", label='3%')

if "ios" in value:
    plt.ylabel("IO requests")
    plt.title("IO requests.")
elif "vol" in value:
    plt.ylabel("Number of messages")
    plt.title("Communication volume.")
else:
    plt.ylabel("Messages' size")
    plt.title("Communication size.")

plt.xticks(ind + 2 * width, names, rotation=45)
plt.legend(loc='best')

plt.grid(linestyle='-', alpha=0.5)

ax = plt.gca()
ax.set_yscale("log", nonposy='clip')

fig = plt.figure(1)
fig.subplots_adjust(bottom=0.2)

if os.path.exists("results/protocol-query-sizes-{0}.pdf".format(value)):
    os.remove("results/protocol-query-sizes-{0}.pdf".format(value))

plt.savefig(
    "results/protocol-query-sizes-{0}.pdf".format(value), format='pdf', dpi=1000)
