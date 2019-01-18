namespace Crypto.Shared
{
	public class NoEncryptionScheme : AbsOPEScheme<BytesKey>
	{
		public NoEncryptionScheme(byte[] seed = null) : base(seed) { }

		public override int Decrypt(OPECipher ciphertext, BytesKey key)
		{
			OnOperation(SchemeOperation.Decrypt);

			return ciphertext.ToInt();
		}

		public override OPECipher Encrypt(int plaintext, BytesKey key)
		{
			OnOperation(SchemeOperation.Encrypt);

			return OPECipher.FromInt(plaintext);
		}

		public override BytesKey KeyGen()
		{
			OnOperation(SchemeOperation.KeyGen);

			return new BytesKey();
		}
	}
}
