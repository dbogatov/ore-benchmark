using ORESchemes.PracticalORE;
using ORESchemes.Shared;
using Xunit;

namespace Test.BPlusTree
{
	[Trait("Category", "Integration")]
	public class PracticalORE : AbsBPlusTree<Ciphertext, BytesKey>
	{
		public PracticalORE() : base(new PracticalOREScheme(new byte[] { 13, 05, 19, 96 }), 500) { }
	}
}
