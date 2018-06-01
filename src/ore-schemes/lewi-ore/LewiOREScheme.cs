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

		/// <summary>
		/// Number of values fit in block
		/// </summary>
		private const int d = 4;
		/// <summary>
		/// Number of blocks
		/// </summary>
		private const int n = 16;

		private readonly int _bitsInBlock;

		public LewiOREScheme(byte[] seed = null) : base(seed)
		{
			F = PRFFactory.GetPRF();
			H = HashFactory.GetHash();
			// P = ((IPRPFactory<uint>)new PRPFactory()).GetPRP();

			_bitsInBlock = (int)Math.Log(d, 2);
		}

		public override byte[] KeyGen()
		{
			OnOperation(SchemeOperation.KeyGen);

			byte[] key = new byte[2 * ALPHA / 8];
			_generator.NextBytes(key);

			// maxCiphertextValue = Encrypt(MaxPlaintextValue(), key);
			// minCiphertextValue = Encrypt(MinPlaintextValue(), key);

			return key;
		}

		public override int Decrypt(Ciphertext ciphertext, byte[] key)
		{
			OnOperation(SchemeOperation.Decrypt);

			return BitConverter.ToInt32(
				F.InversePRF(
					key,
					ciphertext.encrypted
				), 0
			);
		}

		public override Ciphertext Encrypt(int plaintext, byte[] key)
			=> new Ciphertext
			{
				left = EncryptLeft(key, ToUInt(plaintext)),
				right = EncryptRight(key, ToUInt(plaintext))
			};


		protected override int ProperCompare(Ciphertext ciphertextOne, Ciphertext ciphertextTwo)
		{
			Ciphertext.Left left = ciphertextOne.left;
			Ciphertext.Right right = ciphertextOne.right;

			for (int i = 0; i < n; i++)
			{
				byte[] uik = left.pairs[i].Item1;
				int uih = (int)left.pairs[i].Item2;
				
				short z = right.shorts[i][uih];

				short result = (short)((z - new BigInteger(H.ComputeHash(right.nonce, uik))) % 3);

				if (result != 0)
				{
					return result;
				}
			}

			return 0;
		}

		private Ciphertext.Left EncryptLeft(byte[] key, uint input)
		{
			List<Tuple<byte[], uint>> result = new List<Tuple<byte[], uint>>();

			for (int i = 0; i < n; i++)
			{
				int shift = (_bitsInBlock * (n - i - 1));
				uint xi = (input << (_bitsInBlock * i)) >> (_bitsInBlock * (n - 1));
				uint xtoi = input >> shift;

				uint x=0;// = P.PRP(xi, F.DeterministicPRF(key.Skip(256 / 8).ToArray(), BitConverter.GetBytes(xtoi)));

				byte[] xtoix = BitConverter.GetBytes(xtoi).Concat(BitConverter.GetBytes(x)).ToArray();

				byte[] ui = F.DeterministicPRF(key.Take(256 / 8).ToArray(), xtoix);

				result.Add(new Tuple<byte[], uint>(ui, x));
			}

			return new Ciphertext.Left
			{
				pairs = result
			};
		}

		private Ciphertext.Right EncryptRight(byte[] key, uint input)
		{
			List<List<short>> result = new List<List<short>>();

			byte[] nonce = new byte[ALPHA / 8];
			_generator.NextBytes(nonce);

			for (int i = 0; i < n; i++)
			{
				int shift = (_bitsInBlock * (n - i - 1));
				uint yi = (input << (_bitsInBlock * i)) >> (_bitsInBlock * (n - 1));
				uint ytoi = input >> shift;

				List<short> v = new List<short>();

				for (uint j = 1; j <= d; j++)
				{

					uint js = 0;// = P.InversePRP(j, F.DeterministicPRF(key.Skip(256 / 8).ToArray(), BitConverter.GetBytes(ytoi)));

					byte[] ytoij = BitConverter.GetBytes(ytoi).Concat(BitConverter.GetBytes(j)).ToArray();

					short vi = (short)((CMP(js, yi) + new BigInteger(H.ComputeHash(F.DeterministicPRF(key.Take(256 / 8).ToArray(), ytoij), nonce))) % 3);


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

		private short CMP(uint a, uint b)
		{
			if (a < b)
			{
				return -1;
			}
			else if (a == b)
			{
				return 0;
			}
			else
			{
				return 1;
			}
		}

		/// <summary>
		/// Transforms signed int32 to unsigned int32 by shifting the value by int32 min value
		/// </summary>
		private uint ToUInt(int value) => unchecked((uint)(value + Int32.MinValue));
	}
}
