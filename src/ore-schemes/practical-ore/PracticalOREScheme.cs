using System;
using System.Collections.Generic;
using System.Numerics;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives.PRF;

namespace ORESchemes.PracticalORE
{
	public class Ciphertext : IGetSize
	{
		public List<int> tuples = new List<int>();
		public byte[] encrypted;

		public int GetSize() => tuples.Count * 2;
	}

	public class PracticalOREScheme : AbsOREScheme<Ciphertext, BytesKey>
	{
		private readonly int M = 4;
		private readonly IPRF F;
		private readonly byte[] IV;

		public PracticalOREScheme(byte[] seed = null) : base(seed)
		{
			M = Convert.ToInt32(G.Next(4, Int32.MaxValue));
			F = PRFFactory.GetPRF();

			SubscribePrimitive(F);

			IV = new byte[128 / 8];
			G.NextBytes(IV);
		}

		public override int Decrypt(Ciphertext ciphertext, BytesKey key)
		{
			OnOperation(SchemeOperation.Decrypt);

			return BitConverter.ToInt32(
				F.InversePRF(
					key.value,
					ciphertext.encrypted
				), 0
			);
		}

		public override Ciphertext Encrypt(int plaintext, BytesKey key)
		{
			OnOperation(SchemeOperation.Encrypt);

			var result = new Ciphertext();
			result.encrypted = F.PRF(
				key.value,
				BitConverter.GetBytes(plaintext),
				IV
			);

			var unsignedPlaintext = unchecked((uint)plaintext + 1) + Int32.MaxValue;

			for (int i = 0; i < 8 * sizeof(int); i++)
			{
				var shift = (8 * sizeof(int) - i);
				var msg = shift > 31 ? 0 : (unsignedPlaintext >> shift) << shift;
				// https://stackoverflow.com/a/7471843/1644554

				var prfEnc = F.PRF(
					key.value,
					BitConverter.GetBytes(msg),
					IV
				);

				var nextBit = ((unsignedPlaintext << i) >> (8 * sizeof(int) - 1)) & 1;
				var u = (int)((new BigInteger(prfEnc) + nextBit) % M);
				u = u < 0 ? u + M : u;

				result.tuples.Add(u);
			}

			return result;
		}

		protected override bool Compare(Ciphertext ciphertextOne, Ciphertext ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			var length = ciphertextOne.tuples.Count;

			for (int i = 0; i < length; i++)
			{
				var u1 = ciphertextOne.tuples[i];
				var u2 = ciphertextTwo.tuples[i];

				if (u1 != u2)
				{
					return (u2 % M) == ((u1 + 1) % M);
				}
			}

			return false;
		}

		public override BytesKey KeyGen()
		{
			OnOperation(SchemeOperation.KeyGen);

			byte[] key = new byte[ALPHA / 8];
			G.NextBytes(key);

			return new BytesKey(key);
		}
	}
}
