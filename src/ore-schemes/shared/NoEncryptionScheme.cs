using System;
using System.Linq;

namespace ORESchemes.Shared
{
	public class NoEncryptionScheme : AbsOPEScheme
	{
		public NoEncryptionScheme(byte[] seed = null) : base(seed)
		{
			// Set min max ciphertet values
			KeyGen();
		}

		public override int Decrypt(long ciphertext, byte[] key)
		{
			OnOperation(SchemeOperation.Decrypt);

			return (int)ciphertext;
		}

		public override long Encrypt(int plaintext, byte[] key)
		{
			OnOperation(SchemeOperation.Encrypt);

			return (long)plaintext;
		}
	}
}
