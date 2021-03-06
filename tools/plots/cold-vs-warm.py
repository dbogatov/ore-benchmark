#!/usr/bin/env python3

import matplotlib.pyplot as plt
import numpy as np
import os
import sys

fraction = 10

value = str(sys.argv[1])

noencryption = []
bclo = []
clww = []
lewi = []
fhope = []
cloz = []
kerschbaum = []
pope = []
cjjjkrs = []
oram = []

with open("./data/cold-vs-warm-{0}.txt".format(value)) as fp:
    line = fp.readline()
    N = int(line.strip())
    line = fp.readline()
    counter = 0
    while line:
        if counter < N:
            noencryption.append(int(line.strip()))
        elif counter < 2 * N:
            bclo.append(int(line.strip()))
        elif counter < 3 * N:
            clww.append(int(line.strip()))
        elif counter < 4 * N:
            lewi.append(int(line.strip()))
        elif counter < 5 * N:
            cloz.append(int(line.strip()))
        elif counter < 6 * N:
            fhope.append(int(line.strip()))
        elif counter < 7 * N:
            kerschbaum.append(int(line.strip()))
        elif counter < 8 * N:
            pope.append(int(line.strip()))
        elif counter < 9 * N:
            cjjjkrs.append(int(line.strip()))
        elif counter < 10 * N:
            oram.append(int(line.strip()))
        line = fp.readline()
        counter += 1

x = np.arange(N // fraction)

plt.style.use('grayscale')

lewi = lewi[:(N // fraction)]
cloz = cloz[:(N // fraction)]
pope = pope[:(N // fraction)]
cjjjkrs = cjjjkrs[:(N // fraction)]
oram = oram[:(N // fraction)]

# plt.plot(x, noencryption, linewidth=1.0)
# plt.plot(x, bclo, linewidth=1.0)
# plt.plot(x, clww, linewidth=1.0)
plt.plot(x, lewi, marker='x', markersize=2, linewidth=1.0)
# plt.plot(x, fhope, linewidth=1.0)
plt.plot(x, cloz, marker='1', markersize=2, linewidth=1.0)
plt.plot(x, pope, marker='x', markersize=2, zorder=-2, linewidth=1.0)
# plt.plot(x, kerschbaum, linewidth=1.0)
plt.plot(x, cjjjkrs, zorder=-1, linewidth=1.0)
plt.plot(x, oram, linewidth=1.0)

plt.legend(['Lewi-Wu', 'CLOZ', 'POPE', 'Logarithmic\nBRC', 'ORAM'], loc='best')

if os.path.exists("results/cold-vs-warm-{0}.pdf".format(value)):
    os.remove("results/cold-vs-warm-{0}.pdf".format(value))

plt.savefig(
    "results/cold-vs-warm-{0}.pdf".format(value), format='pdf', dpi=1000, bbox_inches='tight', transparent=True)
