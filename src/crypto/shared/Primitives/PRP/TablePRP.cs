using System;
using System.Linq;
using Crypto.Shared.Primitives.PRG;

namespace Crypto.Shared.Primitives.PRP
{
	public class TablePRPFactory : AbsPrimitiveFactory<ISimplifiedPRP>
	{
		protected override ISimplifiedPRP CreatePrimitive(byte[] entropy) => new TablePRP();
	}

	public class TablePRP : AbsPrimitive, ISimplifiedPRP
	{
		private (byte[], byte)? _cacheKey = null;
		private byte[] _cachePermutation;

		public byte PRP(byte input, byte[] key, byte bits)
		{
			GeneratePermutation(key, bits, input);

			return _cachePermutation[input];
		}

		public byte InversePRP(byte input, byte[] key, byte bits)
		{
			GeneratePermutation(key, bits, input);

			return (byte)Array.IndexOf(_cachePermutation, input);
		}

		// https://www.geeksforgeeks.org/shuffle-a-given-array/
		/// <summary>
		/// Generate a permutation of given number of bits and puts it in the cache
		/// </summary>
		/// <param name="key">Key to use as seed for PRG</param>
		/// <param name="bits">Number of bits to permutate</param>
		private void GeneratePermutation(byte[] key, byte bits, byte value)
		{
			if (bits < 1 || bits > 8)
			{
				throw new ArgumentException($"Simplified PRP works only within 1 to {sizeof(byte) * 8} bits.");
			}

			int max = (int)Math.Pow(2, bits) - 1;

			if (value > max)
			{
				throw new ArgumentException($"Invalid input {value} for bitness {bits}.");
			}

			if (!_cacheKey.HasValue || !_cacheKey.Value.Item1 .SequenceEqual(key) || _cacheKey.Value.Item2 != bits)
			{
				OnUse(Primitive.PRP);
				
				_cacheKey = (key, bits);
				_cachePermutation = new byte[sizeof(byte) * 8];

				IPRG G = new PRGCachedFactory(key).GetPrimitive();
				RegisterPrimitive(G);

				_cachePermutation = Enumerable.Range(byte.MinValue, max + 1).Select(v => Convert.ToByte(v)).ToArray();

				// Start from the last element and
				// swap one by one. We don't need to
				// run for the first element 
				// that's why i > 0
				for (int i = max; i > byte.MinValue; i--)
				{
					// Pick a random index
					// from 0 to i
					int j = G.Next(0, i);

					// Swap arr[i] with the
					// element at random index
					byte temp = _cachePermutation[i];
					_cachePermutation[i] = _cachePermutation[j];
					_cachePermutation[j] = temp;
				}
			}
		}
	}
}
