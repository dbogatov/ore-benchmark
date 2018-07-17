using System;
using System.Collections;
using System.Linq;
using ORESchemes.Shared.Primitives.PRF;

namespace ORESchemes.Shared.Primitives.PRP
{
	public interface IPRP : IPRF
	{
		/// <summary>
		/// Perform a pseudo random permutation on bits of input
		/// </summary>
		/// <param name="input">The input to PRP</param>
		/// <param name="key">The key to PRP (source of randomness)</param>
		/// <returns>Permuted bits of input</returns>
		BitArray PRP(BitArray input, byte[] key, int? bits = null);

		/// <summary>
		/// Perform an inverse of pseudo random permutation on bits of input
		/// </summary>
		/// <param name="input">The input to inverse of PRP (output of original PRP)</param>
		/// <param name="key">The key to PRP (source of randomness)</param>
		/// <returns>Un-permuted bits of input</returns>
		BitArray InversePRP(BitArray input, byte[] key, int? bits = null);
	}

	public interface ISimplifiedPRP : IPrimitive
	{
		byte PRP(byte input, byte[] key, byte bits);

		byte InversePRP(byte input, byte[] key, byte bits);
	}

	public abstract class AbsPRP : AbsPrimitive, IPRP
	{
		public byte[] InversePRF(byte[] key, byte[] input)
		{
			BitArray result = InversePRP(new BitArray(input), key);
			byte[] bytes = new byte[(result.Length + 7) / 8];
			result.CopyTo(bytes, 0);

			return bytes;
		}

		public byte[] PRF(byte[] key, byte[] input)
		{
			BitArray result = PRP(new BitArray(input), key);
			byte[] bytes = new byte[(result.Length + 7) / 8];
			result.CopyTo(bytes, 0);

			return bytes;
		}

		public abstract BitArray InversePRP(BitArray input, byte[] key, int? bits = null);

		public abstract BitArray PRP(BitArray input, byte[] key, int? bits = null);
	}
}
