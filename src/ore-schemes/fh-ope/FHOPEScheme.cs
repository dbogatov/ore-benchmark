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
	public class FHOPEScheme : AbsStatefulOPEScheme<State>
	{
		private ulong min;
		private ulong max;

		public FHOPEScheme(long min, long max, byte[] seed = null) : base(seed)
		{
			this.min = min.ToULong();
			this.max = max.ToULong();
		}

		public override IOREScheme<long> Init()
		{
			OnOperation(SchemeOperation.Init);

			byte[] entropy = new byte[266 / 8];
			G.NextBytes(entropy);

			IPRG prg = PRGFactory.GetPRG(entropy);
			SubscribePrimitive(prg);

			State = new State(prg, this.min, this.max);

			maxCiphertextValue = Encrypt(MaxPlaintextValue());
			minCiphertextValue = Encrypt(MinPlaintextValue());

			_minMaxCiphertextsInitialized = true;

			return this;
		}

		public override void Destruct()
		{
			OnOperation(SchemeOperation.Destruct);

			State = null;
		}

		/// <summary>
		/// This scheme is keyless
		/// State is supposed to be the key
		/// </summary>
		public override byte[] KeyGen()
		{
			OnOperation(SchemeOperation.KeyGen);

			return null;
		}

		public override int Decrypt(long ciphertext, byte[] key = null)
		{
			OnOperation(SchemeOperation.Decrypt);
			OnPrimitive(Primitive.TreeTraversal);

			return State.Get(ciphertext.ToULong());
		}

		public override long Encrypt(int plaintext, byte[] key = null)
		{
			OnOperation(SchemeOperation.Encrypt);
			OnPrimitive(Primitive.TreeTraversal);

			return State.Insert(plaintext).ToLong();
		}

		private long MinCiphertext(int plaintext)
		{
			OnPrimitive(Primitive.TreeTraversal);

			return State.GetMinMaxCipher(plaintext, min: true).ToLong();
		}

		private long MaxCiphertext(int plaintext)
		{
			OnPrimitive(Primitive.TreeTraversal);

			return State.GetMinMaxCipher(plaintext, min: false).ToLong();
		}

		/// <summary>
		/// Comparison mechanisms are implemented without a call to generic method
		/// </summary>
		public override bool Compare(long ciphertextOne, long ciphertextTwo) 
			=> throw new InvalidOperationException($"Must not be called on {this.GetType()}");

		public override bool IsEqual(long ciphertextOne, long ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			var plaintextTwo = Decrypt(ciphertextTwo);

			return
				ciphertextOne >= MinCiphertext(plaintextTwo) &&
				ciphertextOne <= MaxCiphertext(plaintextTwo);
		}

		public override bool IsGreater(long ciphertextOne, long ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			var plaintextTwo = Decrypt(ciphertextTwo);

			return ciphertextOne > MaxCiphertext(plaintextTwo);
		}

		public override bool IsLess(long ciphertextOne, long ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			var plaintextTwo = Decrypt(ciphertextTwo);

			return ciphertextOne < MinCiphertext(plaintextTwo);
		}

		public override bool IsGreaterOrEqual(long ciphertextOne, long ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			var plaintextTwo = Decrypt(ciphertextTwo);

			return ciphertextOne >= MinCiphertext(plaintextTwo);
		}

		public override bool IsLessOrEqual(long ciphertextOne, long ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			var plaintextTwo = Decrypt(ciphertextTwo);

			return ciphertextOne <= MaxCiphertext(plaintextTwo);
		}
	}
}
