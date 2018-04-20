using System;
using System.Linq;

namespace ORESchemes.Shared
{
	public class NoEncryptionScheme : IOREScheme<int, int>
	{
		private readonly Random _generator = new Random();

		public event SchemeOperationEventHandler OperationOcurred;

		public int Decrypt(int ciphertext, byte[] key)
		{
			OnOperation(SchemeOperation.Decrypt);

			return ciphertext;
		}

		public void Destruct()
		{
			OnOperation(SchemeOperation.Destruct);

			return;
		}

		public int Encrypt(int plaintext, byte[] key)
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

		public byte[] KeyGen()
		{
			OnOperation(SchemeOperation.KeyGen);

			byte[] key = new byte[256 / 8];
			_generator.NextBytes(key);

			return key;
		}

		public int MaxCiphertextValue()
		{
			return Int32.MaxValue;
		}

		public int MinCiphertextValue()
		{
			return Int32.MinValue;
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
