using ORESchemes.PracticalORE;
using Xunit;

namespace Test.BPlusTree
{
	[Trait("Category", "Integration")]
	public class PracticalORE : AbsBPlusTreeTests<Ciphertext>
	{
		public PracticalORE() : base(new PracticalOREScheme(new byte[] { 13, 05, 19, 96 }), 500) { }
	}
}
