using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives;
using ORESchemes.Shared.Primitives.Hash;
using ORESchemes.Shared.Primitives.PRF;
using ORESchemes.Shared.Primitives.PRG;
using ORESchemes.Shared.Primitives.PRP;

namespace ORESchemes.LewiORE
{
	public class Ciphertext
	{
		public class Left
		{
			public List<Tuple<byte[], uint>> pairs;
		}

		public class Right
		{
			public List<List<short>> shorts;
			public byte[] nonce;
		}

		public byte[] encrypted;
		public Left left;
		public Right right;
	}

	public class LewiOREScheme : AbsORECmpScheme<Ciphertext>
	{
		private readonly IPRF F;
		private readonly IHash H;
		private readonly IPRP P;

		private readonly byte[] IV = new byte[128 / 8];

		/// <summary>
		/// Number of values that fit in a block
		/// </summary>
		private readonly int d = 4;
		/// <summary>
		/// Number of blocks
		/// </summary>
		private readonly int n = 16;

		private readonly int _bitsInBlock;

		public LewiOREScheme(int n = 16, byte[] seed = null) : base(seed)
		{
			if (!new int[] { 16, 8, 4 }.Contains(n))
			{
				throw new ArgumentException($"Value of n ({n}) is invalid. It must be a multiple of 32. One of [16, 8, 4].");
			}

			d = (int)Math.Pow(2, 32 / n);

			F = PRFFactory.GetPRF();
			H = HashFactory.GetHash();
			P = PRPFactory.GetPRP();

			_bitsInBlock = 32 / n;

			_generator.NextBytes(IV);
		}

		public override byte[] KeyGen()
		{
			OnOperation(SchemeOperation.KeyGen);

			byte[] key = new byte[2 * ALPHA / 8];
			_generator.NextBytes(key);

			maxCiphertextValue = Encrypt(MaxPlaintextValue(), key);
			minCiphertextValue = Encrypt(MinPlaintextValue(), key);

			_minMaxCiphertextsInitialized = true;

			return key;
		}

		public override int Decrypt(Ciphertext ciphertext, byte[] key)
		{
			OnOperation(SchemeOperation.Decrypt);

			return BitConverter.ToInt32(
				F.InversePRF(
					key.Take(ALPHA / 8).ToArray(),
					ciphertext.encrypted
				), 0
			);
		}

		public override Ciphertext Encrypt(int plaintext, byte[] key)
		{
			OnOperation(SchemeOperation.Encrypt);

			return new Ciphertext
			{
				left = EncryptLeft(key, ToUInt(plaintext)),
				right = EncryptRight(key, ToUInt(plaintext)),
				encrypted = F.PRF(key.Take(ALPHA / 8).ToArray(), BitConverter.GetBytes(plaintext), IV)
			};
		}


		protected override int ProperCompare(Ciphertext ciphertextOne, Ciphertext ciphertextTwo)
		{
			Ciphertext.Left left = ciphertextOne.left;
			Ciphertext.Right right = ciphertextTwo.right;

			for (int i = 0; i < n; i++)
			{
				byte[] uik = left.pairs[i].Item1;
				int uih = (int)left.pairs[i].Item2;

				short z = right.shorts[i][uih];

				short result = (short)(z - Hash(right.nonce, uik));

				if (result != 0)
				{
					return result;
				}
			}

			return 0;
		}

		/// <summary>
		/// Produces a left side of ciphertext as defined in paper
		/// </summary>
		/// <param name="key">A key with which to encrypt</param>
		/// <param name="input">Input to encrypt</param>
		/// <returns>A left side of ciphertext</returns>
		private Ciphertext.Left EncryptLeft(byte[] key, uint input)
		{
			List<Tuple<byte[], uint>> result = new List<Tuple<byte[], uint>>();

			byte[] k1 = key.Take(ALPHA / 8).ToArray();
			byte[] k2 = key.Skip(ALPHA / 8).ToArray();

			for (int i = 0; i < n; i++)
			{
				int shift = (_bitsInBlock * (n - i));
				uint xi = (input << (_bitsInBlock * i)) >> (_bitsInBlock * (n - 1));
				uint xtoi = shift > 31 ? 0 : input >> shift;

				uint x = Permute(
					xi,
					F.PRF(
						k2,
						BitConverter.GetBytes(xtoi),
						IV
					)
				);

				byte[] xtoix = Concatenate(xtoi, x);

				byte[] ui = F.PRF(k1, xtoix, IV);

				result.Add(new Tuple<byte[], uint>(ui, x));
			}

			return new Ciphertext.Left
			{
				pairs = result
			};
		}

		/// <summary>
		/// Produces a right side of ciphertext as defined in paper
		/// </summary>
		/// <param name="key">A key with which to encrypt</param>
		/// <param name="input">Input to encrypt</param>
		/// <returns>A right side of ciphertext</returns>
		private Ciphertext.Right EncryptRight(byte[] key, uint input)
		{
			List<List<short>> result = new List<List<short>>();

			byte[] nonce = new byte[ALPHA / 8];
			_generator.NextBytes(nonce);

			byte[] k1 = key.Take(ALPHA / 8).ToArray();
			byte[] k2 = key.Skip(ALPHA / 8).ToArray();

			for (int i = 0; i < n; i++)
			{
				int shift = (_bitsInBlock * (n - i));
				uint yi = (input << (_bitsInBlock * i)) >> (_bitsInBlock * (n - 1));
				uint ytoi = shift > 31 ? 0 : input >> shift;

				List<short> v = new List<short>();

				for (uint j = 0; j < d; j++)
				{
					uint js = Unpermute(
						j,
						F.PRF(
							k2,
							BitConverter.GetBytes(ytoi),
							IV
						)
					);

					byte[] ytoij = Concatenate(ytoi, j);

					var cmp = CMP(js, yi);
					var hash = Hash(nonce, F.PRF(k1, ytoij, IV));

					short vi = (short)(cmp + hash);

					v.Add(vi);
				}

				result.Add(v);
			}

			return new Ciphertext.Right
			{
				shorts = result,
				nonce = nonce
			};
		}

		/// <summary>
		/// Helper function that implements CMP as defined in the paper
		/// </summary>
		private int CMP(uint a, uint b)
			=> (a < b) ? -1 : ((a == b) ? 0 : 1);

		/// <summary>
		/// Transforms signed int32 to unsigned int32 by shifting the value by int32 min value
		/// </summary>
		private uint ToUInt(int value) => unchecked((uint)(value + Int32.MinValue));

		/// <summary>
		/// Wrapper around PRP permute function
		/// </summary>
		private uint Permute(uint input, byte[] key)
		{
			BitArray permutation =
				P.PRP(
					new BitArray(new int[] { (int)input }),
					key,
					_bitsInBlock
				);
			int[] result = new int[1];
			permutation.CopyTo(result, 0);

			return (uint)result[0];
		}

		/// <summary>
		/// Wrapper around PRP unpermute function
		/// </summary>
		private uint Unpermute(uint input, byte[] key)
		{
			BitArray permutation =
				P.InversePRP(
					new BitArray(new int[] { (int)input }),
					key,
					_bitsInBlock
				);
			int[] result = new int[1];
			permutation.CopyTo(result, 0);

			return (uint)result[0];
		}

		/// <summary>
		/// Wrapper around Hash function
		/// </summary>
		/// <returns>Hash modulo 3</returns>
		private int Hash(byte[] input, byte[] key)
			=> (int)(BigInteger.Abs(new BigInteger(H.ComputeHash(input, key))) % 3);

		/// <summary>
		/// Helper that produces bytes concatenation of its inputs
		/// </summary>
		private byte[] Concatenate(uint first, uint second)
			=> BitConverter.GetBytes(first).Concat(BitConverter.GetBytes(second)).ToArray();
	}
}
