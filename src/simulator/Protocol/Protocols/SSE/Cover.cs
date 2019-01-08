using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ORESchemes.Shared;

namespace Simulation.Protocol.SSE
{
	public static class Cover
	{
		private static bool[] BitsFromUInt(uint x)
		{
			var bitArray = new BitArray(BitConverter.GetBytes(x));
			bool[] bits = new bool[bitArray.Length];
			bitArray.CopyTo(bits, 0);

			return bits;
		}

		private static BitArray ExtractRange(bool[] input, int from, int to)
		{
			var result = new bool[to - from + 1];
			for (int i = from; i <= to; i++)
			{
				result[i - from] = input[i];
			}

			return new BitArray(result);
		}

		public static List<(BitArray, int)> Path(uint x, int n = -1)
		{
			var xBits = BitsFromUInt(x);

			n = n < 0 ? xBits.Length : n;

			var Theta = new List<(BitArray, int)>();

			for (int i = 0; i < n; i++)
			{
				Theta.Add((ExtractRange(xBits, 0, i), n - i - 1));
			}
			Theta.Add((new BitArray(new bool[] { }), n));

			return Theta;
		}

		public static List<(BitArray, int)> BRC(uint a, uint b, int n = -1)
		{


			// tau <- {}
			var Tau = new List<(BitArray, int)>();

			var aBits = BitsFromUInt(a);
			var bBits = BitsFromUInt(b);
			n = n < 0 ? aBits.Length : n;

			if (a > b)
			{
				throw new ArgumentException($"a ({a}) > b {b}");
			}
			if (a == b)
			{
				return new List<(BitArray, int)> { (ExtractRange(aBits, 0, n - 1), 0) };
			}

			// t <- max{i | a_i != b_i}
			var t = -1;
			for (int i = n - 1; i >= 0; i--)
			{
				if (aBits[i] != bBits[i])
				{
					t = i;
					break;
				}
			}

			Debug.Assert(t > -1);

			// if (forall i <= t : a_i = 0) then
			var line3condition = true;
			for (int i = 0; i <= t; i++)
			{
				if (aBits[i])
				{
					line3condition = false;
					break;
				}
			}

			if (line3condition)
			{
				// if (forall i <= t : b_i = 1) then
				var line4condition = true;
				for (int i = 0; i <= t; i++)
				{
					if (!bBits[i])
					{
						line4condition = false;
						break;
					}
				}

				if (line4condition)
				{
					// return ((a_{n−1} ... a_{t+1}), t + 1)
					return new List<(BitArray, int)> { (ExtractRange(aBits, t + 1, n - 1), t + 1) };
				}
				else
				{
					// Append((a_{n−1} ... a_t), t) to tau
					Tau.Add((ExtractRange(aBits, t, n - 1), t));
				}
			}
			else
			{
				// mu <- min{i | i < t AND a_i = 1}
				var mu = -1;
				for (int i = 0; i < t; i++)
				{
					if (aBits[i])
					{
						mu = i;
						break;
					}
				}

				Debug.Assert(mu > -1);

				// for i = t − 1 to mu + 1
				for (int i = t - 1; i >= mu + 1; i--)
				{
					// if a_i = 0 then
					if (!aBits[i])
					{
						// Append ((a_{n−1} ... a_{i+1} 1), i) to tau
						var toAppend = ExtractRange(aBits, i + 1, n - 1);
						toAppend = toAppend.Prepend(new BitArray(new bool[] { true }));
						Tau.Add((toAppend, i));
					}
				}

				// Append ((a_{n−1} ... a_{mu}), mu) to tau
				Tau.Add((ExtractRange(aBits, mu, n - 1), mu));
			}

			// if (forall i <= t : b_i = 1) then
			var line14condition = true;
			for (int i = 0; i <= t; i++)
			{
				if (!bBits[i])
				{
					line14condition = false;
					break;
				}
			}

			if (line14condition)
			{
				// Append ((b_{nu−1} ... b_t), t) to tau
				// WARNING: possible typo in algorithm (mu <=> n)
				Tau.Add((ExtractRange(bBits, t, n - 1), t));
			}
			else
			{
				// nu <- min{i | i < t AND b_i = 0}
				var nu = -1;
				for (int i = 0; i < t; i++)
				{
					if (!bBits[i])
					{
						nu = i;
						break;
					}
				}

				Debug.Assert(nu > -1);

				// for i = t − 1 to nu + 1
				for (int i = t - 1; i >= nu + 1; i--)
				{
					// if b_i = 1 then
					if (bBits[i])
					{
						// Append ((b_{n−1} ... b_{i+1} 0), i) to tau
						var toAppend = ExtractRange(bBits, i + 1, n - 1);
						toAppend = toAppend.Prepend(new BitArray(new bool[] { false }));
						Tau.Add((toAppend, i));
					}
				}

				// Append ((b{n−1} ... b_{nu}), nu) to tau
				Tau.Add((ExtractRange(bBits, nu, n - 1), nu));
			}

			Debug.Assert(Tau.Count > 0);

			return Tau;
		}
	}
}
