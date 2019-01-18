using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Crypto.Shared;
using Crypto.Shared.Primitives.Hash;
using Crypto.Shared.Primitives.PRF;
using Crypto.Shared.Primitives.PRP;

namespace Crypto.LewiWu
{
	public class Key : IGetSize
	{
		public byte[] left = new byte[128 / 8];
		public byte[] right = new byte[128 / 8];

		public int GetSize() => (left.Length + right.Length) * sizeof(byte) * 8;
	}

	public class Ciphertext : IGetSize
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

		public int GetSize() =>
			(left != null ? left.pairs.Sum(p => p.Item1.Length * sizeof(byte) * 8 + sizeof(uint) * 8) : 0) +
			(right != null ?
				right.nonce.Length * sizeof(byte) * 8 +
				right.shorts.Sum(s => s.Count) * 2
			: 0);
	}

	public class Scheme : AbsORECmpScheme<Ciphertext, Key>
	{
		private readonly IPRF F;
		private readonly IHash H;
		private readonly ISimplifiedPRP P;

		/// <summary>
		/// Number of values that fit in a block
		/// </summary>
		private readonly int d = 4;
		/// <summary>
		/// Number of blocks
		/// </summary>
		private readonly int n = 16;

		private readonly int _bitsInBlock;

		public Scheme(int n = 16, byte[] seed = null) : base(seed)
		{
			if (!new int[] { 16, 8, 4 }.Contains(n))
			{
				throw new ArgumentException($"Value of n ({n}) is invalid. It must be a factor of 32. One of [16, 8, 4].");
			}

			this.n = n;
			d = (int)Math.Pow(2, 32 / n);

			F = new PRFFactory().GetPrimitive();
			H = new HashFactory().GetPrimitive();
			P = new TablePRPFactory().GetPrimitive();

			SubscribePrimitive(F);
			SubscribePrimitive(H);
			SubscribePrimitive(P);

			_bitsInBlock = 32 / n;
		}

		public override Key KeyGen()
		{
			OnOperation(SchemeOperation.KeyGen);

			Key key = new Key();
			G.NextBytes(key.left);
			G.NextBytes(key.right);

			return key;
		}

		public override int Decrypt(Ciphertext ciphertext, Key key)
		{
			OnOperation(SchemeOperation.Decrypt);

			return BitConverter.ToInt32(
				E.Decrypt(
					key.left,
					ciphertext.encrypted
				), 0
			);
		}

		public override Ciphertext Encrypt(int plaintext, Key key)
		{
			return new Ciphertext
			{
				left = EncryptLeft(key.left, key.right, ToUInt(plaintext)),
				right = EncryptRight(key.left, key.right, ToUInt(plaintext)),
				encrypted = E.Encrypt(key.left, BitConverter.GetBytes(plaintext))
			};
		}


		protected override int ProperCompare(Ciphertext ciphertextOne, Ciphertext ciphertextTwo)
		{
			bool invert = false;

			Ciphertext.Left left;
			Ciphertext.Right right;

			if (ciphertextOne.left != null && ciphertextTwo.right != null)
			{
				left = ciphertextOne.left;
				right = ciphertextTwo.right;
			}
			else if (ciphertextOne.right != null && ciphertextTwo.left != null)
			{
				left = ciphertextTwo.left;
				right = ciphertextOne.right;
				invert = true;
			}
			else
			{
				throw new InvalidOperationException($"One of the ciphers must have left part, another must have right part.");
			}

			for (int i = 0; i < n; i++)
			{
				byte[] uik = left.pairs[i].Item1;
				int uih = (int)left.pairs[i].Item2;

				short z = right.shorts[i][uih];

				short result = (short)(z - Hash(right.nonce, uik));

				if (result != 0)
				{
					if (invert)
					{
						// Flip sign if inverted
						result *= -1;
					}
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
		public Ciphertext.Left EncryptLeft(byte[] leftKey, byte[] rightKey, uint input)
		{
			OnOperation(SchemeOperation.Encrypt);

			List<Tuple<byte[], uint>> result = new List<Tuple<byte[], uint>>();

			for (int i = 0; i < n; i++)
			{
				int shift = (_bitsInBlock * (n - i));
				uint xi = (input << (_bitsInBlock * i)) >> (_bitsInBlock * (n - 1));
				uint xtoi = shift > 31 ? 0 : input >> shift;

				uint x = P.PRP(
					(byte)xi,
					F.PRF(
						rightKey,
						BitConverter.GetBytes(xtoi)
					),
					(byte)_bitsInBlock
				);

				byte[] xtoix = Concatenate(xtoi, x);

				byte[] ui = F.PRF(leftKey, xtoix);

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
		public Ciphertext.Right EncryptRight(byte[] leftKey, byte[] rightKey, uint input)
		{
			OnOperation(SchemeOperation.Encrypt);

			List<List<short>> result = new List<List<short>>();

			byte[] nonce = new byte[ALPHA / 8];
			G.NextBytes(nonce);

			for (int i = 0; i < n; i++)
			{
				int shift = (_bitsInBlock * (n - i));
				uint yi = (input << (_bitsInBlock * i)) >> (_bitsInBlock * (n - 1));
				uint ytoi = shift > 31 ? 0 : input >> shift;

				List<short> v = new List<short>();

				for (uint j = 0; j < d; j++)
				{
					uint js = P.InversePRP(
						(byte)j,
						F.PRF(
							rightKey,
							BitConverter.GetBytes(ytoi)
						),
						(byte)_bitsInBlock
					);

					byte[] ytoij = Concatenate(ytoi, j);

					var cmp = CMP(js, yi);
					var hash = Hash(nonce, F.PRF(leftKey, ytoij));

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
