using Crypto.CLOZ;
using Xunit;

namespace Test.BPlusTree
{
	[Trait("Category", "Integration")]
	public class CLOZ : AbsBPlusTree<Ciphertext, Key>
	{
		public CLOZ() : base(new Scheme(new byte[] { 13, 05, 19, 96 }), 100) { }
	}
}
