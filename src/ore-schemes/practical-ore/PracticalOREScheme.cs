using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives;
using ORESchemes.Shared.Primitives.PRF;

namespace ORESchemes.PracticalORE
{
	public class Ciphertext
	{
		public List<int> tuples = new List<int>();
		public byte[] encrypted;
	}

	public class PracticalOREScheme : AbsOREScheme<Ciphertext>
	{
		private readonly int M = 4;
		private readonly IPRF F;

		public PracticalOREScheme(byte[] seed = null) : base(seed)
		{
			M = Convert.ToInt32(_generator.Next(4, Int32.MaxValue));
			F = PRFFactory.GetPRF();
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
		{
			OnOperation(SchemeOperation.Encrypt);

			var result = new Ciphertext();
			result.encrypted = F.PRF(
				key,
				BitConverter.GetBytes(plaintext)
			);

			var unsignedPlaintext = unchecked((uint)plaintext + 1) + Int32.MaxValue;

			for (int i = 0; i < 8 * sizeof(int); i++)
			{
				var shift = (8 * sizeof(int) - i);
				var msg = shift > 31 ? 0 : (unsignedPlaintext >> shift) << shift;
				// https://stackoverflow.com/a/7471843/1644554

				var prfEnc = F.DeterministicPRF(
					key,
					BitConverter.GetBytes(msg)
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

			if (ciphertextOne.tuples.Count != ciphertextTwo.tuples.Count)
			{
				throw new InvalidOperationException("Malformed ciphertexts");
			}

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
	}
}
