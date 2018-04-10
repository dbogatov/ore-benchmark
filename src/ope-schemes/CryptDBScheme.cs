using System;

namespace OPESchemes
{
	public class CryptDBScheme : IOPEScheme<int, int>
	{
		public event SchemeOperationEventHandler OperationOcurred;

		public int Decrypt(int ciphertext, int key)
		{
			throw new NotImplementedException();
		}

		public void Destruct()
		{
			throw new NotImplementedException();
		}

		public int Encrypt(int plaintext, int key)
		{
			throw new NotImplementedException();
		}

		public void Init()
		{
			throw new NotImplementedException();
		}

		public bool IsEqual(int ciphertextOne, int ciphertextTwo)
		{
			throw new NotImplementedException();
		}

		public bool IsGreater(int ciphertextOne, int ciphertextTwo)
		{
			throw new NotImplementedException();
		}

		public bool IsGreaterOrEqual(int ciphertextOne, int ciphertextTwo)
		{
			throw new NotImplementedException();
		}

		public bool IsLess(int ciphertextOne, int ciphertextTwo)
		{
			throw new NotImplementedException();
		}

		public bool IsLessOrEqual(int ciphertextOne, int ciphertextTwo)
		{
			throw new NotImplementedException();
		}

		public int KeyGen()
		{
			throw new NotImplementedException();
		}

		public int MaxCiphertextValue()
		{
			throw new NotImplementedException();
		}

		public int MinCiphertextValue()
		{
			throw new NotImplementedException();
		}
	}
}
