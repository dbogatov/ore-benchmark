using System;
using System.Linq;

namespace ORESchemes.Shared
{
	public class NoEncryptionScheme : AbsOPEScheme<object>
	{
		public NoEncryptionScheme(byte[] seed = null) : base(seed) { }

		public override int Decrypt(long ciphertext, object key)
		{
			OnOperation(SchemeOperation.Decrypt);

			return (int)ciphertext;
		}

		public override long Encrypt(int plaintext, object key)
		{
			OnOperation(SchemeOperation.Encrypt);

			return (long)plaintext;
		}

		public override object KeyGen()
		{
			OnOperation(SchemeOperation.KeyGen);

			return new object();
		}
	}
}
