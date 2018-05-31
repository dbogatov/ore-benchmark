using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives;

namespace ORESchemes.LewiORE
{
	public class Ciphertext
	{
		public List<int> left = new List<int>();
		public List<int> right = new List<int>();
		public byte[] encrypted;
	}

	public class LewiOREScheme : AbsORECmpScheme<Ciphertext>
	{
		private readonly IPRF F;
		private readonly IHash H;
		private readonly IPRP<int> P;

		private readonly byte[] IV;

		public LewiOREScheme(byte[] seed = null) : base(seed)
		{
			F = PRFFactory.GetPRF();
			H = HashFactory.GetHash();
			P = ((IPRPFactory<int>)new PRPFactory()).GetPRP();

			IV = new byte[128 / 8];
			_generator.NextBytes(IV);
		}

		public override byte[] KeyGen()
		{
			OnOperation(SchemeOperation.KeyGen);

			byte[] key = new byte[2 * ALPHA / 8];
			_generator.NextBytes(key);

			maxCiphertextValue = Encrypt(MaxPlaintextValue(), key);
			minCiphertextValue = Encrypt(MinPlaintextValue(), key);

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
		{
			throw new NotImplementedException();
		}

		protected override int ProperCompare(Ciphertext ciphertextOne, Ciphertext ciphertextTwo)
		{
			throw new NotImplementedException();
		}
	}
}
