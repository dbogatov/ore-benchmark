using System;
using ORESchemes.PracticalORE;
using Xunit;

namespace Test.ORESchemes
{
	[Trait("Category", "Unit")]
	public class PracticalORETests : GenericORETests<Ciphertext>
	{
		protected override void SetScheme()
		{
			_scheme = new PracticalOREScheme(_entropy);
		}
	}
}
