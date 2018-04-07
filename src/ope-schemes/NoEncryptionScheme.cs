using System;
using System.Linq;

namespace OPESchemes
{
	public class NoEncryptionScheme : IOPEScheme<int, int>
	{
		private readonly Random generator = new Random();

		public event SchemeOperationEventHandler OperationOcurred;

		public int Decrypt(int ciphertext, int key)
		{
			OnOperation(SchemeOperation.Decrypt);

			return ciphertext;
		}

		public void Destruct()
		{
			OnOperation(SchemeOperation.Destruct);

			return;
		}

		public int Encrypt(int plaintext, int key)
		{
			OnOperation(SchemeOperation.Encrypt);

			return plaintext;
		}

		public void Init()
		{
			OnOperation(SchemeOperation.Init);

			return;
		}

		public bool IsEqual(int ciphertextOne, int ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			return ciphertextOne == ciphertextTwo;
		}

		public bool IsGreater(int ciphertextOne, int ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			return ciphertextOne > ciphertextTwo;
		}

		public bool IsGreaterOrEqual(int ciphertextOne, int ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			return ciphertextOne >= ciphertextTwo;
		}

		public bool IsLess(int ciphertextOne, int ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			return ciphertextOne < ciphertextTwo;
		}

		public bool IsLessOrEqual(int ciphertextOne, int ciphertextTwo)
		{
			OnOperation(SchemeOperation.Comparison);

			return ciphertextOne <= ciphertextTwo;
		}

		public int KeyGen()
		{
			OnOperation(SchemeOperation.KeyGen);

			return generator.Next(Int32.MaxValue);
		}

		public int MaxCiphertextValue()
		{
			return Int32.MaxValue;
		}

		public int MinCiphertextValue()
		{
			return Int32.MinValue;
		}

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
