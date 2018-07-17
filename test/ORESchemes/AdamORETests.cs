using ORESchemes.AdamORE;
using Xunit;

namespace Test.ORESchemes
{
	[Trait("Category", "Unit")]
	public class AdamORE : GenericORE<Ciphertext, Key>
	{
		protected override void SetScheme()
		{
			_scheme = new AdamOREScheme(_entropy);
		}

		public override int CipherSize() => 4216;
		public override int KeySize() => 384;
	}
}
