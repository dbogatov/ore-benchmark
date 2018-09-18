#!/usr/bin/env python

import matplotlib.pyplot as plt
import numpy as np
import os
import sys

value = str(sys.argv[1])

noencryption = []
bclo = []
clww = []
lewi = []
fhope = []
cloz = []
kerschbaum = []
pope = []

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
            fhope.append(int(line.strip()))
        elif counter < 6 * N:
            cloz.append(int(line.strip()))
        elif counter < 7 * N:
            kerschbaum.append(int(line.strip()))
        elif counter < 8 * N:
            pope.append(int(line.strip()))
        line = fp.readline()
        counter += 1

x = np.arange(N)

plt.style.use('grayscale')

plt.plot(x, noencryption)
plt.plot(x, bclo)
plt.plot(x, clww)
plt.plot(x, lewi)
plt.plot(x, fhope)
plt.plot(x, cloz)
plt.plot(x, kerschbaum)
plt.plot(x, pope, marker='x', markersize=4)

plt.legend(['No encryption', 'BCLO', 'CLWW', 'Lewi-Wu', 'FH-OPE',
            'CLOZ', 'Kerschbaum', 'POPE'], loc='best')

if os.path.exists("results/cold-vs-warm-{0}.pdf".format(value)):
    os.remove("results/cold-vs-warm-{0}.pdf".format(value))

plt.savefig(
    "results/cold-vs-warm-{0}.pdf".format(value), format='pdf', dpi=1000)
