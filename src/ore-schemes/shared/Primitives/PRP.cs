using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ORESchemes.Shared.Primitives
{
	/// <typeparam name="T">PRP input / output type</typeparam>
	public interface IPRPFactory<T>
	{
		/// <summary>
		/// Returns an initialized instance of a PRP
		/// </summary>
		IPRP<T> GetPRP();

		/// <summary>
		/// Returns an initialized instance of a strong PRP
		/// </summary>
		IPRP<T> GetStrongPRP();

	}

	public class PRPFactory : IPRPFactory<byte[]>, IPRPFactory<short>, IPRPFactory<int>, IPRPFactory<long>
	{
		IPRP<byte[]> IPRPFactory<byte[]>.GetPRP() => new Feistel(3);

		IPRP<byte[]> IPRPFactory<byte[]>.GetStrongPRP() => new Feistel(4);

		IPRP<short> IPRPFactory<short>.GetPRP() => new Feistel(3);

		IPRP<short> IPRPFactory<short>.GetStrongPRP() => new Feistel(4);

		IPRP<int> IPRPFactory<int>.GetPRP() => new Feistel(3);

		IPRP<int> IPRPFactory<int>.GetStrongPRP() => new Feistel(4);

		IPRP<long> IPRPFactory<long>.GetPRP() => new Feistel(3);

		IPRP<long> IPRPFactory<long>.GetStrongPRP() => new Feistel(4);
	}

	/// <typeparam name="T">PRP input / output type</typeparam>
	public interface IPRP<T>
	{
		/// <summary>
		/// Perform a pseudo random permutation on bits of input
		/// </summary>
		/// <param name="input">The input to PRP</param>
		/// <param name="key">The key to PRP (source of randomness)</param>
		/// <returns>Permuted bits of input</returns>
		T PRP(T input, byte[] key);

		/// <summary>
		/// Perform an inverse of pseudo random permutation on bits of input
		/// </summary>
		/// <param name="input">The input to inverse of PRP (output of original PRP)</param>
		/// <param name="key">The key to PRP (source of randomness)</param>
		/// <returns>Un-permuted bits of input</returns>
		T InversePRP(T input, byte[] key);
	}

	public abstract class AbsPRP : IPRP<byte[]>, IPRP<short>, IPRP<int>, IPRP<long>, IPRF
	{
		public abstract byte[] InversePRP(byte[] input, byte[] key);
		public abstract byte[] PRP(byte[] input, byte[] key);

		public short InversePRP(short input, byte[] key) =>
			BitConverter.ToInt16(InversePRP(BitConverter.GetBytes(input), key), 0);

		public short PRP(short input, byte[] key) =>
			BitConverter.ToInt16(PRP(BitConverter.GetBytes(input), key), 0);

		public int InversePRP(int input, byte[] key) =>
			BitConverter.ToInt32(InversePRP(BitConverter.GetBytes(input), key), 0);

		public int PRP(int input, byte[] key) =>
			BitConverter.ToInt32(PRP(BitConverter.GetBytes(input), key), 0);

		public long InversePRP(long input, byte[] key) =>
			BitConverter.ToInt64(InversePRP(BitConverter.GetBytes(input), key), 0);

		public long PRP(long input, byte[] key) =>
			BitConverter.ToInt64(PRP(BitConverter.GetBytes(input), key), 0);

		public byte[] PRF(byte[] key, byte[] input, byte[] IV = null) => PRP(input, key);

		public byte[] InversePRF(byte[] key, byte[] input) => InversePRP(input, key);
	}

	/// <summary>
	/// PRP based on multiple-round Feistel networks 
	/// https://en.wikipedia.org/wiki/Feistel_cipher
	/// </summary>
	public class Feistel : AbsPRP
	{
		private readonly int _rounds;
		private readonly IPRF _prf;

		public Feistel(int rounds = 3)
		{
			_rounds = rounds;
			_prf = PRFFactory.GetPRF();
		}

		public override byte[] InversePRP(byte[] input, byte[] key)
		{
			return Permute(input, key, true);
		}

		public override byte[] PRP(byte[] input, byte[] key)
		{
			return Permute(input, key);
		}

		/// <summary>
		/// Perform all round of permutation (spliting and merging inputs)
		/// </summary>
		/// <param name="input">Input to PRP</param>
		/// <param name="key">Key to PRP</param>
		/// <param name="inverse">True, if inverse of PRP requested</param>
		/// <returns>Permuted or un-permuted bits of input</returns>
		/// <remark>Input must be an even number of bytes</remark>
		private byte[] Permute(byte[] input, byte[] key, bool inverse = false)
		{
			if (input.Length % 2 != 0)
			{
				throw new ArgumentException("Input must be an even number of bytes.");
				// input = input.Concat(new byte[] { 0x00 }).ToArray();
			}

			Tuple<byte[], byte[]> round = Split(input);

			if (inverse)
			{
				round = Swap(round);
			}

			for (int i = 0; i < _rounds; i++)
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
		/// Performa a single round of Feistel cipher
		/// </summary>
		/// <param name="input">Input to PRP</param>
		/// <param name="key">Key to PRP</param>
		/// <returns>Permuted bits of input</returns>
		private Tuple<byte[], byte[]> Round(Tuple<byte[], byte[]> input, byte[] key)
		{
			if (input.Item1.Length != input.Item2.Length)
			{
				throw new ArgumentException("Lengths must be equal");
			}

			int length = input.Item1.Length;

			Tuple<byte[], byte[]> result = new Tuple<byte[], byte[]>(new byte[length], new byte[length]);

			Array.Copy(input.Item2, result.Item1, length);
			Array.Copy(
				Xor(input.Item1, _prf.PRF(key, input.Item2, key.Take(128 / 8).ToArray())),
				result.Item2,
				length
			);

			return result;
		}

		/// <summary>
		/// Returns result of applying XOR operation on two byte arrays
		/// Returns the array with length minimum of two inputs
		/// </summary>
		private byte[] Xor(byte[] left, byte[] right)
		{
			var length = Math.Min(left.Length, right.Length);
			byte[] result = new byte[length];

			for (int i = 0; i < length; i++)
			{
				result[i] = (byte)(left[i] ^ right[i]);
			}

			return result;
		}

		/// <summary>
		/// Helper function that splits byte array into tuple of two byte arrays
		/// </summary>
		private Tuple<byte[], byte[]> Split(byte[] input) =>
			new Tuple<byte[], byte[]>(
				input.Take(input.Length / 2).ToArray(),
				input.Skip(input.Length / 2).ToArray()
			);

		/// <summary>
		/// Helper function that merges a tuple of two byte arrays into one byte array
		/// </summary>
		private byte[] Merge(Tuple<byte[], byte[]> input) =>
			input.Item1.Concat(input.Item2).ToArray();

		/// <summary>
		/// Helper function that swaps two byte arrays of a tuple
		/// </summary>
		private Tuple<byte[], byte[]> Swap(Tuple<byte[], byte[]> input)
		{
			int length = input.Item1.Length;

			byte[] buff = new byte[length];
			Array.Copy(input.Item1, buff, length);
			Array.Copy(input.Item2, input.Item1, length);
			Array.Copy(buff, input.Item2, length);

			return input;
		}
	}
}
