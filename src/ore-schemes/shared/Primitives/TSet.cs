using System;
using System.Collections;
using System.Collections.Generic;
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

	/// <summary>
	/// An abstraction over keyword
	/// </summary>
	public interface IWord
	{
		/// <summary>
		/// A keyword will be put through PRF, so byte representation is required
		/// </summary>
		byte[] ToBytes();
	}

	/// <summary>
	/// A particular keyword implementation based on primitive strings
	/// </summary>
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

	public class TSetStructure
	{
		public Record[][] Set { get; set; }
		public int B { get; set; }
	}

	/// <summary>
	/// Tuple set construction as defined in https://eprint.iacr.org/2013/169.pdf section 2.2
	/// </summary>
	public interface ITSet : IPrimitive
	{
		/// <summary>
		/// Generates an encrypted index data structure that holds correspondence
		/// between keywords and encrypted indices of documents
		/// </summary>
		/// <param name="T">Mapping of keywords to lists of encrypted indices</param>
		/// <returns>An encrypted index data structure and a secret key</returns>
		ValueTuple<TSetStructure, byte[]> Setup(Dictionary<IWord, BitArray[]> T);

		/// <summary>
		/// Generates a token to search over encrypted index data structure
		/// </summary>
		/// <param name="Kt">A secret key generated by Setup</param>
		/// <param name="w">A keyword</param>
		/// <remarks>A keyword must be one of those supplied to Setup</remarks>  
		/// <returns>A token to search over encrypted index data structure</returns>
		byte[] GetTag(byte[] Kt, IWord w);

		/// <summary>
		/// Returns a list of encrypted indices associated with the keyword corresponding to the token
		/// </summary>
		/// <param name="TSet">An encrypted index data structure (generated with Setup)</param>
		/// <param name="stag">A token generated with Setup</param>
		/// <returns>A list of encrypted indices</returns>
		BitArray[] Retrive(TSetStructure TSet, byte[] stag);
	}

	public class CashTSet : AbsPrimitive, ITSet
	{
		/// <summary>
		/// Thrown when overlow in a bucket occurs.
		/// If so, Setup is restarted with a fresh key.
		/// </summary>
		private class OverflowException : Exception { }

		private readonly IPRG G;
		private readonly IPRF F;
		private readonly IHash H;

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
			OnUse(Primitive.TSet);

			// Output stag <- F^-(Kt , w)
			return F.PRF(Kt, w.ToBytes());
		}

		public BitArray[] Retrive(TSetStructure TSet, byte[] stag)
		{
			OnUse(Primitive.TSet);

			// Initialize t as an empty list, bit Beta as 1, and counter i as 1
			var t = new List<BitArray>();
			var Beta = true;
			var i = 0;

			// Repeat the following loop while Beta = 1:
			while (Beta)
			{
				// Set (b, L, K) <- H(F(stag, i)) and retrieve an array B <- TSet[b]
				(var b, var L, var K) = DecomposeFromHash(stag, i, TSet.B);
				var B = TSet.Set[b];

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

		public (TSetStructure, byte[]) Setup(Dictionary<IWord, BitArray[]> T)
		{
			OnUse(Primitive.TSet);
			
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
							(var b, var L, var K) = DecomposeFromHash(stag, i, B);

							// If Free[b] is an empty set, restart TSetSetup(T) with fresh key Kt .
							var size = Free[b].Count();
							if (size == 0)
							{
								throw new OverflowException();
							}

							// Choose j <-$ Free[b] and remove j from set Free[b], i.e. set Free[b] <- Free[b] \ {j}.
							// https://stackoverflow.com/a/15960061/1644554
							var j = Free[b].ElementAt(size == 1 ? 0 : G.Next(0, size - 1));
							Free[b].Remove(j);

							// Set bit Beta as 1 if i < |t| and 0 if i = |t|.
							var Beta = i < t.Count() - 1;

							// Set TSet[b, j].label <- L and TSet[b, j].value <- (Beta|si) XOR K.
							TSet[b][j] = new Record
							{
								Label = new BitArray(L),
								Value = si.Prepend(new BitArray(new bool[] { Beta })).Xor(K)
							};
						}
					}

					// Output (TSet, Kt).
					return (new TSetStructure { Set = TSet, B = B }, Kt);
				}
				catch (OverflowException)
				{
					// Oops, overflow
					// If it happens too often (non-negligibly often), consider different B and S
					continue;
				}
			}
		}

		/// <summary>
		/// Run hash function on token and index and decompose the result into three components
		/// </summary>
		/// <param name="stag">A token</param>
		/// <param name="i">An index</param>
		/// <param name="B">A global B value chosen in Setup</param>
		/// <returns>A tuple of: number from 0 to B exclusive, 128-bits string and 129-bits string</returns>
		private ValueTuple<int, BitArray, BitArray> DecomposeFromHash(byte[] stag, int i, int B)
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

			b = Math.Abs(b % B);

			return (b, LBits, KBits);
		}
	}
}