using Crypto.Shared;
using Xunit;

namespace Test.BPlusTree
{
	[Trait("Category", "Unit")]
	public class NoEncryption : AbsBPlusTree<OPECipher, BytesKey>
	{
		public NoEncryption() : base(new NoEncryptionScheme(new byte[] { 13, 05, 19, 96 })) { }
	}
}
