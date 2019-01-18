using Crypto.LewiWu;
using Xunit;

namespace Test.BPlusTree
{
	[Trait("Category", "Integration")]
	public class LewiWu : AbsBPlusTree<Ciphertext, Key>
	{
		public LewiWu() : base(new Scheme(16, new byte[] { 13, 05, 19, 96 }), 100) { }
	}
}
