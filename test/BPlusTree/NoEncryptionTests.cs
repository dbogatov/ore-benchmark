using ORESchemes.Shared;
using Xunit;

namespace Test.BPlusTree
{
	[Trait("Category", "Unit")]
	public class NoEncryption : AbsBPlusTreeTests<long>
	{
		public NoEncryption() : base(new NoEncryptionScheme(new byte[] { 13, 05, 19, 96 })) { }
	}
}
