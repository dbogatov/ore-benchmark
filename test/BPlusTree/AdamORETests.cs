using ORESchemes.AdamORE;
using Xunit;

namespace Test.BPlusTree
{
	[Trait("Category", "Integration")]
	public class AdamORE : AbsBPlusTree<Ciphertext, Key>
	{
		public AdamORE() : base(new AdamOREScheme(new byte[] { 13, 05, 19, 96 }), 500) { }
	}
}
