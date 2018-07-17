using System;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives;
using ORESchemes.Shared.Primitives.PRG;

namespace ORESchemes.FHOPE
{
	public class Ciphertext : IGetSize
	{
		public long? min = null;
		public long? max = null;
		public long value;

		public int GetSize() =>
			sizeof(long) +
			(min.HasValue ? 0 : sizeof(long)) +
			(max.HasValue ? 0 : sizeof(long));
	}

	public class FHOPEScheme : AbsOREScheme<Ciphertext, State>
	{
		private ulong min;
		private ulong max;

		private int r;
		private double p;

		public FHOPEScheme(long min, long max, int r = 10, double p = 0.0, byte[] seed = null) : base(seed)
		{
			this.min = min.ToULong();
			this.max = max.ToULong();

			this.p = p;
			this.r = r;
		}

		public override State KeyGen()
		{
			OnOperation(SchemeOperation.KeyGen);

			byte[] entropy = new byte[128 / 8];
			G.NextBytes(entropy);

			IPRG prg = new PRGCachedFactory(entropy).GetPrimitive();
			SubscribePrimitive(prg);

			State state = new State(prg, this.min, this.max, r, p);

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

			return new Ciphertext { value = key.Insert(plaintext).ToLong() };
		}

		/// <summary>
		/// Returns the smallest ciphertext from the state for the given plaintext
		/// </summary>
		public long MinCiphertext(int plaintext, State key)
		{
			OnPrimitive(Primitive.TreeTraversal);

			return key.GetMinMaxCipher(plaintext, min: true).ToLong();
		}

		/// <summary>
		/// Returns the largest ciphertext from the state for the given plaintext
		/// </summary>
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

			if (!(ciphertextOne.min.HasValue && ciphertextOne.max.HasValue))
			{
				CiphertextCheck(ciphertextTwo);

				return
					ciphertextOne.value >= ciphertextTwo.min &&
					ciphertextOne.value <= ciphertextTwo.max;
			}
			else
			{
				CiphertextCheck(ciphertextOne);

				return
					ciphertextOne.min <= ciphertextTwo.value &&
					ciphertextOne.max >= ciphertextTwo.value;
			}
		}

		public override bool IsGreater(Ciphertext ciphertextOne, Ciphertext ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			if (!(ciphertextOne.min.HasValue && ciphertextOne.max.HasValue))
			{
				CiphertextCheck(ciphertextTwo);

				return ciphertextOne.value > ciphertextTwo.max;
			}
			else
			{
				CiphertextCheck(ciphertextOne);

				return ciphertextOne.min > ciphertextTwo.value;
			}
		}

		public override bool IsLess(Ciphertext ciphertextOne, Ciphertext ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			if (!(ciphertextOne.min.HasValue && ciphertextOne.max.HasValue))
			{
				CiphertextCheck(ciphertextTwo);

				return ciphertextOne.value < ciphertextTwo.min;
			}
			else
			{
				CiphertextCheck(ciphertextOne);

				return ciphertextOne.max < ciphertextTwo.value;
			}
		}

		public override bool IsGreaterOrEqual(Ciphertext ciphertextOne, Ciphertext ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			if (!(ciphertextOne.min.HasValue && ciphertextOne.max.HasValue))
			{
				CiphertextCheck(ciphertextTwo);

				return ciphertextOne.value >= ciphertextTwo.min;
			}
			else
			{
				CiphertextCheck(ciphertextOne);

				return ciphertextOne.max >= ciphertextTwo.value;
			}
		}

		public override bool IsLessOrEqual(Ciphertext ciphertextOne, Ciphertext ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			if (!(ciphertextOne.min.HasValue && ciphertextOne.max.HasValue))
			{
				CiphertextCheck(ciphertextTwo);

				return ciphertextOne.value <= ciphertextTwo.max;
			}
			else
			{
				CiphertextCheck(ciphertextOne);

				return ciphertextOne.min <= ciphertextTwo.value;
			}
		}

		/// <summary>
		/// Throws exception if ciphertext's min or max are not set.
		/// </summary>
		private void CiphertextCheck(Ciphertext cipher)
		{
			if (!cipher.min.HasValue || !cipher.max.HasValue)
			{
				throw new ArgumentNullException("Ciphertext's min and max must be set for comparison");
			}
		}
	}
}
