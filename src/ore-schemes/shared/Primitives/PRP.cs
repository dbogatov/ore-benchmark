using System;
using System.Collections;
using System.Linq;
using ORESchemes.Shared.Primitives.PRF;

namespace ORESchemes.Shared.Primitives.PRP
{
	public class PRPFactory : AbsPrimitiveFactory<IPRP>
	{
		protected override IPRP CreatePrimitive(byte[] entropy)
		{
			return new Feistel(3);
		}
	}

	public class StrongPRPFactory : AbsPrimitiveFactory<IPRP>
	{
		protected override IPRP CreatePrimitive(byte[] entropy)
		{
			return new Feistel(4);
		}
	}

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

	/// <summary>
	/// PRP based on multiple-round Feistel networks 
	/// https://en.wikipedia.org/wiki/Feistel_cipher
	/// </summary>
	public class Feistel : AbsPRP
	{
		public readonly int Rounds;
		private readonly IPRF F;

		public Feistel(int rounds = 3)
		{
			Rounds = rounds;
			F = new PRFFactory().GetPrimitive();

			F.PrimitiveUsed += new PrimitiveUsageEventHandler(
				(prim, impure) => base.OnUse(prim, true)
			);
		}

		public override BitArray InversePRP(BitArray input, byte[] key, int? bits = null)
		{
			OnUse(Primitive.PRP);

			return Permute(input, key, bits ?? input.Length, true);
		}

		public override BitArray PRP(BitArray input, byte[] key, int? bits = null)
		{
			OnUse(Primitive.PRP);

			return Permute(input, key, bits ?? input.Length);
		}

		/// <summary>
		/// Perform all round of permutation (spliting and merging inputs)
		/// </summary>
		/// <param name="input">Input to PRP</param>
		/// <param name="key">Key to PRP</param>
		/// <param name="inverse">True, if inverse of PRP requested</param>
		/// <returns>Permuted or un-permuted bits of input</returns>
		/// <remark>Input must be an even number of bytes</remark>
		private BitArray Permute(BitArray input, byte[] key, int bits, bool inverse = false)
		{
			input = new BitArray(input.Cast<bool>().Take(bits).ToArray());
			Tuple<BitArray, BitArray> round = Split(input, input.Length / 2);

			if (inverse)
			{
				round = Swap(round);
			}

			for (int i = 0; i < Rounds; i++)
			{
				round = Round(round, key);
			}

			if (inverse)
			{
				round = Swap(round);
			}

			return Merge(round);
		}

		/// <summary>
		/// Perform a single round of Feistel cipher
		/// </summary>
		/// <param name="input">Input to PRP</param>
		/// <param name="key">Key to PRP</param>
		/// <returns>Permuted bits of input</returns>
		private Tuple<BitArray, BitArray> Round(Tuple<BitArray, BitArray> input, byte[] key)
		{
			int length = input.Item1.Length;
			byte[] bytes = new byte[(input.Item1.Length + 7) / 8];
			input.Item1.CopyTo(bytes, 0);

			Tuple<BitArray, BitArray> result = new Tuple<BitArray, BitArray>(
				Xor(
					input.Item2,
					new BitArray(
						F.PRF(key, bytes).Skip(128 / 8).ToArray()
					)
				),
				input.Item1
			);

			return result;
		}

		/// <summary>
		/// Returns result of applying XOR operation on two bit arrays
		/// Returns the array with length minimum of two inputs
		/// </summary>
		private BitArray Xor(BitArray left, BitArray right)
		{
			int length = Math.Min(left.Length, right.Length);

			return new BitArray(left.Cast<bool>().Take(length).ToArray())
			.Xor(new BitArray(right.Cast<bool>().Take(length).ToArray()));
		}

		/// <summary>
		/// Helper function that splits bit array into tuple of two bit arrays
		/// </summary>
		private Tuple<BitArray, BitArray> Split(BitArray input, int bits)
			=> new Tuple<BitArray, BitArray>(
				new BitArray(input.Cast<bool>().Take(bits).ToArray()),
				new BitArray(input.Cast<bool>().Skip(bits).ToArray())
			);

		/// <summary>
		/// Helper function that merges a tuple of two bit arrays into one bit array
		/// </summary>
		private BitArray Merge(Tuple<BitArray, BitArray> input) =>
			new BitArray(
				input.Item1.Cast<bool>()
					.Concat(input.Item2.Cast<bool>())
					.ToArray()
			);

		/// <summary>
		/// Helper function that returns new tuple with items swapped
		/// </summary>
		private Tuple<BitArray, BitArray> Swap(Tuple<BitArray, BitArray> input)
			=> new Tuple<BitArray, BitArray>(input.Item2, input.Item1);
	}
}
