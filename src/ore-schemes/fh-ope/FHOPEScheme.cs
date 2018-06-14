using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives;
using ORESchemes.Shared.Primitives.PRG;

namespace ORESchemes.FHOPE
{
	public class Ciphertext
	{
		public long min;
		public long max;
		public long value;
	}

	public class FHOPEScheme : AbsOREScheme<Ciphertext, State>
	{
		private ulong min;
		private ulong max;

		public FHOPEScheme(long min, long max, byte[] seed = null) : base(seed)
		{
			this.min = min.ToULong();
			this.max = max.ToULong();
		}

		/// <summary>
		/// This scheme is keyless
		/// State is supposed to be the key
		/// </summary>
		public override State KeyGen()
		{
			OnOperation(SchemeOperation.KeyGen);

			byte[] entropy = new byte[256 / 8];
			G.NextBytes(entropy);

			IPRG prg = PRGFactory.GetPRG(entropy);
			SubscribePrimitive(prg);

			State state = new State(prg, this.min, this.max);

			maxCiphertextValue = Encrypt(MaxPlaintextValue(), state);
			minCiphertextValue = Encrypt(MinPlaintextValue(), state);

			_minMaxCiphertextsInitialized = true;

			return state;
		}

		public override int Decrypt(Ciphertext ciphertext, State key)
		{
			OnOperation(SchemeOperation.Decrypt);
			OnPrimitive(Primitive.TreeTraversal);

			return key.Get(ciphertext.value.ToULong());
		}

		public override Ciphertext Encrypt(int plaintext, State key)
		{
			OnOperation(SchemeOperation.Encrypt);
			OnPrimitive(Primitive.TreeTraversal);

			var cipher = key.Insert(plaintext).ToLong();

			return new Ciphertext
			{
				value = cipher,
				max = key.GetMinMaxCipher(plaintext, min: false).ToLong(),
				min = key.GetMinMaxCipher(plaintext, min: true).ToLong()
			};
		}

		public long MinCiphertext(int plaintext, State key)
		{
			OnPrimitive(Primitive.TreeTraversal);

			return key.GetMinMaxCipher(plaintext, min: true).ToLong();
		}

		public long MaxCiphertext(int plaintext, State key)
		{
			OnPrimitive(Primitive.TreeTraversal);

			return key.GetMinMaxCipher(plaintext, min: false).ToLong();
		}

		/// <summary>
		/// Comparison mechanisms are implemented without a call to generic method
		/// </summary>
		protected override bool Compare(Ciphertext ciphertextOne, Ciphertext ciphertextTwo)
			=> throw new InvalidOperationException($"Must not be called on {this.GetType()}");

		public override bool IsEqual(Ciphertext ciphertextOne, Ciphertext ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			return
				ciphertextOne.value >= ciphertextTwo.min &&
				ciphertextOne.value <= ciphertextTwo.max;
		}

		public override bool IsGreater(Ciphertext ciphertextOne, Ciphertext ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			return ciphertextOne.value > ciphertextTwo.max;
		}

		public override bool IsLess(Ciphertext ciphertextOne, Ciphertext ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			return ciphertextOne.value < ciphertextTwo.min;
		}

		public override bool IsGreaterOrEqual(Ciphertext ciphertextOne, Ciphertext ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			return ciphertextOne.value >= ciphertextTwo.min;
		}

		public override bool IsLessOrEqual(Ciphertext ciphertextOne, Ciphertext ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			return ciphertextOne.value <= ciphertextTwo.max;
		}
	}
}
