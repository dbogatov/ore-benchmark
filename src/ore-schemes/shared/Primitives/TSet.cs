using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ORESchemes.Shared.Primitives.Hash;
using ORESchemes.Shared.Primitives.PRF;
using ORESchemes.Shared.Primitives.PRG;

namespace ORESchemes.Shared.Primitives.TSet
{
	public class TSetFactory : AbsPrimitiveFactory<ITSet>
	{
		protected override ITSet CreatePrimitive(byte[] entropy) => new CashTSet();
	}

	public interface IWord
	{
		byte[] ToBytes();
	}

	public class StringWord : IWord
	{
		public string Value { get; set; }

		public byte[] ToBytes() => Encoding.Default.GetBytes(Value);

		public override int GetHashCode() => Value.GetHashCode();
	}

	public class Record
	{
		public BitArray Label { get; set; } // 128 bit
		public BitArray Value { get; set; } // 129 bit
	}

	public interface ITSet : IPrimitive
	{
		ValueTuple<Record[][], byte[]> Setup(Dictionary<IWord, BitArray[]> T);
		byte[] GetTag(byte[] Kt, IWord w);
		BitArray[] Retrive(Record[][] TSet, byte[] stag);
	}

	public class CashTSet : AbsPrimitive, ITSet
	{
		private class OverflowException : Exception { }

		private readonly IPRG G;
		private readonly IPRF F;
		private readonly IHash H;

		private int? B;
		private int? S;

		public CashTSet(byte[] entropy = null)
		{
			G = new PRGFactory(entropy).GetPrimitive();
			F = new PRFFactory().GetPrimitive();
			H = new Hash512Factory().GetPrimitive();

			RegisterPrimitive(G);
			RegisterPrimitive(F);
			RegisterPrimitive(H);
		}

		public byte[] GetTag(byte[] Kt, IWord w)
		{
			// Output stag <- F^-(Kt , w)
			return F.PRF(Kt, w.ToBytes());
		}

		public BitArray[] Retrive(Record[][] TSet, byte[] stag)
		{
			// Initialize t as an empty list, bit Beta as 1, and counter i as 1
			var t = new List<BitArray>();
			var Beta = true;
			var i = 0;

			// Repeat the following loop while Beta = 1:
			while (Beta)
			{
				// Set (b, L, K) <- H(F(stag, i)) and retrieve an array B <- TSet[b]
				(var b, var L, var K) = DecomposeFromHash(stag, i);
				var B = TSet[b];

				// Search for index j in {1, ..., S} s.t. B[j].label = L.
				var jFound = -1;
				for (int j = 0; j < B.Count(); j++)
				{
					if (B[j] != null && B[j].Label.IsEqualTo(L))
					{
						jFound = j;
						break;
					}
				}

				if (jFound < 0)
				{
					throw new InvalidOperationException($"No j such that B[j].label = L (i = {i}). Most likely, malformed word supplied.");
				}

				// Let v <- B[j].value XOR K. Let Beta be the first bit of v, and s the remaining n(λ) bits of v
				var v = B[jFound].Value.Xor(K);
				Beta = v[0];

				var s = new BitArray(Enumerable.Repeat(false, 128).ToArray());
				for (int j = 1; j < 128 + 1; j++)
				{
					s[j - 1] = v[j];
				}

				// Add string s to the list t and increment i.
				t.Add(s);
				i++;
			}

			// Output t.
			return t.ToArray();
		}

		public (Record[][], byte[]) Setup(Dictionary<IWord, BitArray[]> T)
		{
			var failures = 0;
			while (true)
			{
				try // until no overflow
				{
					// The TSetSetup(T) procedure sets the parameters B and S depending on the total number N
					var N = T.Sum(v => v.Value.Count());
					var S = (int)Math.Ceiling(3 * Math.Log(N, 2));
					var B = 2 * N / S;

					// At least some reasonable minimum values
					S = S >= 5 ? S : 5;
					B = B >= 5 ? B : 5;

					this.S = S;
					this.B = B;

					// Initialize an array TSet of size B whose every element is an array of S records of type record
					var TSet = new Record[B][];
					for (int i = 0; i < B; i++)
					{
						TSet[i] = new Record[S];
					}

					// Initialize an array Free of size B whose elements are integer sets, initially all set to {1, ..., S}.
					var Free = new HashSet<int>[B];
					for (int i = 0; i < B; i++)
					{
						Free[i] = new HashSet<int>(Enumerable.Range(0, S));
					}

					// Choose a random key Kt of PRF F^-
					var Kt = G.GetBytes(128 / 8);

					// Let W be the set of keywords in DB. For every w in W do the following:
					foreach (var pair in T)
					{
						var w = pair.Key;
						var t = pair.Value;

						// Set stag <- F^-(Kt , w) and t <- T[w].
						var stag = F.PRF(Kt, w.ToBytes());

						// For each i = 1, ..., |t|, set s_i as the i-th string in t, and perform the following steps:
						for (int i = 0; i < t.Count(); i++)
						{
							var si = t[i];

							// Set (b, L, K) <- H(F(stag, i))
							(var b, var L, var K) = DecomposeFromHash(stag, i);

							// If Free[b] is an empty set, restart TSetSetup(T) with fresh key Kt .
							var size = Free[b].Count();
							if (size == 0)
							{
								throw new OverflowException();
							}

							// Choose j <-$ Free[b] and remove j from set Free[b], i.e. set Free[b] <- Free[b] \ {j}.
							// https://stackoverflow.com/a/15960061/1644554
							var j = Free[b].ElementAt(size == 1 ? 0 : G.Next(0, size - 1));

							// Set bit Beta as 1 if i < |t| and 0 if i = |t|.
							var Beta = i < t.Count() - 1;

							// Set TSet[b, j].label <- L and TSet[b, j].value <- (Beta|si) XOR K.
							TSet[b][j] = new Record
							{
								Label = new BitArray(L),
								Value = si.Prepend(new BitArray(new bool[] { Beta })).Xor(K)
							};
						}

						// Output (TSet, Kt).
						return (TSet, Kt);
					}
				}
				catch (OverflowException)
				{
					// Oops, overflow
					// If it happens too often (non-negligibly often), consider different B and S

					failures++;
					if (failures >= 10)
					{
						throw new InvalidOperationException($"TSet.Setup fails too much - {failures} failures in a row.");
					}
					continue;
				}
			}
		}

		private ValueTuple<int, BitArray, BitArray> DecomposeFromHash(byte[] stag, int i)
		{
			var input = stag.Concat(BitConverter.GetBytes(i)).ToArray();

			var output = H.ComputeHash(input);

			var bits = new BitArray(output);

			var bBits = new BitArray(Enumerable.Repeat(false, sizeof(int) * 8).ToArray());
			for (int j = 0; j < sizeof(int) * 8; j++)
			{
				bBits[j] = bits[j];
			}

			var LBits = new BitArray(Enumerable.Repeat(false, 128).ToArray());
			for (int j = sizeof(int) * 8; j < sizeof(int) * 8 + 128; j++)
			{
				LBits[j - sizeof(int) * 8] = bits[j];
			}

			var KBits = new BitArray(Enumerable.Repeat(false, 129).ToArray());
			for (int j = sizeof(int) * 8 + 128; j < sizeof(int) * 8 + 128 + 129; j++)
			{
				KBits[j - (sizeof(int) * 8 + 128)] = bits[j];
			}

			// https://stackoverflow.com/a/5283199/1644554
			int[] bArray = new int[1];
			bBits.CopyTo(bArray, 0);
			int b = bArray[0];

			if (!this.B.HasValue)
			{
				throw new InvalidOperationException("TSet.Setup must have been called first.");
			}

			b = Math.Abs(b % this.B.Value);

			return (b, LBits, KBits);
		}
	}
}
