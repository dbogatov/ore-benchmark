using System;
using System.Linq;

namespace OPESchemes
{
	public class NoEncryptionScheme : IOPEScheme
	{
		private readonly int keyLength = 16;
		private readonly Random generator = new Random();

		public string Decrypt(string ciphertext, string key)
		{
			return ciphertext;
		}

		public void Destruct()
		{
			return;
		}

		public string Encrypt(string plaintext, string key)
		{
			return plaintext;
		}

		public void Init()
		{
			return;
		}

		public bool IsEqual(string ciphertextOne, string ciphertextTwo)
		{
			return ciphertextOne.Equals(ciphertextTwo, StringComparison.Ordinal);
		}

		public bool IsGreater(string ciphertextOne, string ciphertextTwo)
		{
			return string.Compare(ciphertextOne, ciphertextTwo, true) > 0;
		}

		public string KeyGen()
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			
			return 
				new string(
					Enumerable
						.Repeat(chars, keyLength)
						.Select(s => s[generator.Next(s.Length)])
						.ToArray()
					);

		}
	}
}
