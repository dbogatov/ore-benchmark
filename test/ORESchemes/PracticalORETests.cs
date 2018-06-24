using ORESchemes.PracticalORE;
using ORESchemes.Shared;
using Xunit;

namespace Test.ORESchemes
{
	[Trait("Category", "Unit")]
	public class PracticalORETests : GenericORETests<Ciphertext, BytesKey>
	{
		protected override void SetScheme()
		{
			_scheme = new PracticalOREScheme(_entropy);
		}

		public override int CipherSize() => 32 * 2;
	}
}
