using System;
using System.Linq;

namespace OPESchemes
{
	public class NoEncryptionScheme : IOPEScheme
	{
		private readonly Random generator = new Random();

		public int Decrypt(int ciphertext, int key)
		{
			return ciphertext;
		}

		public void Destruct()
		{
			return;
		}

		public int Encrypt(int plaintext, int key)
		{
			return plaintext;
		}

		public void Init()
		{
			return;
		}

		public bool IsEqual(int ciphertextOne, int ciphertextTwo)
		{
			return ciphertextOne == ciphertextTwo;
		}

		public bool IsGreater(int ciphertextOne, int ciphertextTwo)
		{
			return ciphertextOne > ciphertextTwo;
		}

		public int KeyGen()
		{
			return generator.Next(Int32.MaxValue);
		}
	}
}
