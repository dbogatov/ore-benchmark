using ORESchemes.LewiORE;
using Xunit;

namespace Test.BPlusTree
{
	[Trait("Category", "Integration")]
	public class LewiORE : AbsBPlusTree<Ciphertext, Key>
	{
		public LewiORE() : base(new LewiOREScheme(16, new byte[] { 13, 05, 19, 96 }), 100) { }
	}
}
