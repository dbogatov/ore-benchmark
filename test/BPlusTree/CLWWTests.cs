using Crypto.CLWW;
using Crypto.Shared;
using Xunit;

namespace Test.BPlusTree
{
	[Trait("Category", "Integration")]
	public class CLWW : AbsBPlusTree<Ciphertext, BytesKey>
	{
		public CLWW() : base(new Scheme(new byte[] { 13, 05, 19, 96 }), 500) { }
	}
}
