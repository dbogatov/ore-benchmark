using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ORESchemes.Shared.Primitives
{
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

	public interface IPRP<T>
	{
		T PRP(T input, byte[] key);

		T InversePRP(T input, byte[] key);
	}

	public abstract class AbsPRP : IPRP<byte[]>, IPRP<short>, IPRP<int>, IPRP<long>
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
	}

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

		public byte[] Permute(byte[] input, byte[] key, bool inverse = false)
		{
			if (input.Length % 2 != 0)
			{
				throw new ArgumentException("Input must be an even number of bytes.");
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

		private Tuple<byte[], byte[]> Split(byte[] input) =>
			new Tuple<byte[], byte[]>(
				input.Take(input.Length / 2).ToArray(),
				input.Skip(input.Length / 2).ToArray()
			);

		private byte[] Merge(Tuple<byte[], byte[]> input) =>
			input.Item1.Concat(input.Item2).ToArray();

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
