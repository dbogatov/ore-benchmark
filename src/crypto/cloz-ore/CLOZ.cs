using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Crypto.Shared;
using Crypto.Shared.Primitives.PPH;
using Crypto.Shared.Primitives.PRF;
using Crypto.Shared.Primitives.PRP;

namespace Crypto.CLOZ
{
	public class Key : IGetSize
	{
		public byte[] encryptionKey;
		public Shared.Primitives.PPH.Key pphKey;

		public int GetSize() => 8 * encryptionKey.Length + pphKey.GetSize();
	}

	public class Ciphertext : IGetSize
	{
		public List<byte[]> tuples = new List<byte[]>();
		public byte[] encrypted;
		public byte[] testKey;

		public int GetSize() => tuples.Sum(t => t.Length) * 8;
	}

	public class Scheme : AbsORECmpScheme<Ciphertext, Key>
	{
		private readonly IPRF F;
		private readonly IPPH R;
		private readonly ISimplifiedPRP P;

		public Scheme(byte[] seed = null) : base(seed)
		{
			F = new PRFFactory().GetPrimitive();
			R = new PPHFactory(G.GetBytes(ALPHA / 8)).GetPrimitive();
			P = new TablePRPFactory().GetPrimitive();

			SubscribePrimitive(F);
			SubscribePrimitive(R);
			SubscribePrimitive(P);
		}

		public override int Decrypt(Ciphertext ciphertext, Key key)
		{
			OnOperation(SchemeOperation.Decrypt);

			return BitConverter.ToInt32(
				E.Decrypt(
					key.encryptionKey,
					ciphertext.encrypted
				), 0
			);
		}

		public override Ciphertext Encrypt(int plaintext, Key key)
		{
			OnOperation(SchemeOperation.Encrypt);

			var result = new Ciphertext
			{
				encrypted = E.Encrypt(
					key.encryptionKey,
					BitConverter.GetBytes(plaintext)
				),
				testKey = key.pphKey.testKey
			};

			var unsignedPlaintext = unchecked((uint)plaintext + 1) + Int32.MaxValue;

			byte[][] tuples = new byte[8 * sizeof(int)][];
			byte[] permutationKey = G.GetBytes(ALPHA / 8);

			for (int i = 0; i < 8 * sizeof(int); i++)
			{
				var shift = (8 * sizeof(int) - i);
				var msg = shift > 31 ? 0 : (unsignedPlaintext >> shift) << shift;
				// https://stackoverflow.com/a/7471843/1644554

				var prfEnc = F.PRF(
					key.encryptionKey,
					BitConverter.GetBytes(msg).Concat(BitConverter.GetBytes(i)).ToArray()
				);

				var nextBit = ((unsignedPlaintext << i) >> (8 * sizeof(int) - 1)) & 1;

				var u = (
					(new BigInteger(prfEnc) + nextBit) %
					BigInteger.Pow(2, 128)
				).ToByteArray();

				tuples[P.PRP((byte)i, permutationKey, 5)] = R.Hash(key.pphKey.hashKey, u);
			}

			result.tuples = tuples.ToList();

			return result;
		}

		protected override int ProperCompare(Ciphertext ciphertextOne, Ciphertext ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			var key = ciphertextOne.testKey;

			for (int i = 0; i < 8 * sizeof(int); i++)
			{
				for (int j = 0; j < 8 * sizeof(int); j++)
				{
					var v1 = ciphertextOne.tuples[i];
					var v2 = ciphertextTwo.tuples[j];

					if (R.Test(key, v1, v2))
					{
						return 1;
					}
					else if (R.Test(key, v2, v1))
					{
						return -1;
					}
				}
			}

			return 0;
		}

		public override Key KeyGen()
		{
			OnOperation(SchemeOperation.KeyGen);

			return new Key
			{
				encryptionKey = G.GetBytes(ALPHA / 8),
				pphKey = R.KeyGen()
			};
		}
	}
}
