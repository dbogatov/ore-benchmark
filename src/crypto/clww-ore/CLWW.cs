﻿using System;
using System.Collections.Generic;
using System.Numerics;
using Crypto.Shared;
using Crypto.Shared.Primitives.PRF;

namespace Crypto.CLWW
{
	public class Ciphertext : IGetSize
	{
		public List<int> tuples = new List<int>();
		public byte[] encrypted;

		public int GetSize() => tuples.Count * 2;
	}

	public class Scheme : AbsOREScheme<Ciphertext, BytesKey>
	{
		private readonly int M = 3;
		private readonly IPRF F;

		public Scheme(byte[] seed = null) : base(seed)
		{
			F = new PRFFactory().GetPrimitive();

			SubscribePrimitive(F);
		}

		public override int Decrypt(Ciphertext ciphertext, BytesKey key)
		{
			OnOperation(SchemeOperation.Decrypt);

			return BitConverter.ToInt32(
				E.Decrypt(
					key.value,
					ciphertext.encrypted
				), 0
			);
		}

		public override Ciphertext Encrypt(int plaintext, BytesKey key)
		{
			OnOperation(SchemeOperation.Encrypt);

			var result = new Ciphertext();
			result.encrypted = E.Encrypt(
				key.value,
				BitConverter.GetBytes(plaintext)
			);

			var unsignedPlaintext = unchecked((uint)plaintext + 1) + Int32.MaxValue;

			for (int i = 0; i < 8 * sizeof(int); i++)
			{
				var shift = (8 * sizeof(int) - i);
				var msg = shift > 31 ? 0 : (unsignedPlaintext >> shift) << shift;
				// https://stackoverflow.com/a/7471843/1644554

				var prfEnc = F.PRF(
					key.value,
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
