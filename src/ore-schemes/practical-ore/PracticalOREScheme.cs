using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives;

namespace ORESchemes.PracticalORE
{
	public class Ciphertext
	{
		public List<int> tuples = new List<int>();
		public byte[] encrypted;

		public override bool Equals(object obj)
		{
			// Check for null values and compare run-time types.
			if (obj == null || GetType() != obj.GetType())
				return false;

			Ciphertext c = (Ciphertext)obj;
			return this.tuples.Zip(c.tuples, (c1, c2) => c1 == c2).All(eq => eq);
		}

		public override int GetHashCode()
		{
			return tuples.GetHashCode();
		}
	}

	public class PracticalOREScheme : IOREScheme<int, Ciphertext>
	{
		private readonly Random _generator = new Random();
		private readonly int _alpha = 128;

		private Ciphertext maxCiphertextValue = null;
		private Ciphertext minCiphertextValue = null;

		private readonly int M = 4;
		private readonly IPRF F;
		private readonly byte[] IV;

		public event SchemeOperationEventHandler OperationOcurred;

		public PracticalOREScheme(int alpha, int seed)
		{
			_generator = new Random(seed);
			_alpha = alpha;
			M = Convert.ToInt32(_generator.Next(4, Int32.MaxValue));
			F = PRFFactory.GetPRF();

			IV = new byte[_alpha / 8];
			_generator.NextBytes(IV);
		}

		public int Decrypt(Ciphertext ciphertext, byte[] key)
		{
			OnOperation(SchemeOperation.Decrypt);

			return BitConverter.ToInt32(
				F.InversePRF(
					key,
					ciphertext.encrypted
				), 0
			);
		}

		public void Destruct()
		{
			OnOperation(SchemeOperation.Destruct);

			return;
		}

		public Ciphertext Encrypt(int plaintext, byte[] key)
		{
			OnOperation(SchemeOperation.Encrypt);

			var result = new Ciphertext();
			result.encrypted = F.PRF(
				key,
				BitConverter.GetBytes(plaintext)
			);

			var unsignedPlaintext = unchecked((uint)plaintext) + Int32.MaxValue;

			for (int i = 0; i < 8 * sizeof(int); i++)
			{
				var shift = (8 * sizeof(int) - i);
				var msg = shift > 31 ? 0 : (unsignedPlaintext >> shift) << shift;
				// https://stackoverflow.com/a/7471843/1644554

				var prfEnc = F.PRF(
					key,
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

		public void Init()
		{
			OnOperation(SchemeOperation.Init);

			return;
		}

		/// <summary>
		/// m1 < m2
		/// </summary>
		/// <param name="ciphertextOne"></param>
		/// <param name="ciphertextTwo"></param>
		/// <returns></returns>
		private bool Compare(Ciphertext ciphertextOne, Ciphertext ciphertextTwo)
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
					var res1 = (u2 % M);
					var res2 = ((u1 + 1) % M);
					var result = res1 == res2;
					var lolo = (res1 == res2);
					return result;
				}
			}

			return false;
		}

		public bool IsEqual(Ciphertext ciphertextOne, Ciphertext ciphertextTwo)
		{
			return
				!IsLess(ciphertextOne, ciphertextTwo) &&
				!IsLess(ciphertextTwo, ciphertextOne);
		}

		public bool IsGreater(Ciphertext ciphertextOne, Ciphertext ciphertextTwo)
		{
			return
				!IsLess(ciphertextOne, ciphertextTwo) &&
				!IsEqual(ciphertextOne, ciphertextTwo);
		}

		public bool IsGreaterOrEqual(Ciphertext ciphertextOne, Ciphertext ciphertextTwo)
		{
			return !IsLess(ciphertextOne, ciphertextTwo);
		}

		public bool IsLess(Ciphertext ciphertextOne, Ciphertext ciphertextTwo)
		{
			return Compare(ciphertextOne, ciphertextTwo);
		}

		public bool IsLessOrEqual(Ciphertext ciphertextOne, Ciphertext ciphertextTwo)
		{
			return !IsGreater(ciphertextOne, ciphertextTwo);
		}

		public byte[] KeyGen()
		{
			OnOperation(SchemeOperation.KeyGen);

			byte[] key = new byte[_alpha / 8];
			_generator.NextBytes(key);

			maxCiphertextValue = Encrypt(Int32.MaxValue, key);
			minCiphertextValue = Encrypt(Int32.MinValue, key);

			return key;
		}

		public Ciphertext MaxCiphertextValue()
		{
			if (maxCiphertextValue == null)
			{
				throw new InvalidOperationException("Max value is generated during KeyGen operation");
			}

			return maxCiphertextValue;
		}

		public Ciphertext MinCiphertextValue()
		{
			if (minCiphertextValue == null)
			{
				throw new InvalidOperationException("Min value is generated during KeyGen operation");
			}

			return minCiphertextValue;
		}

		/// <summary>
		/// Emits the event that scheme performed an operation
		/// </summary>
		/// <param name="operation">The operation that scheme performed</param>
		private void OnOperation(SchemeOperation operation)
		{
			var handler = OperationOcurred;
			if (handler != null)
			{
				handler(operation);
			}
		}
	}
}
