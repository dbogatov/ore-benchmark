using System;
using ORESchemes.LewiORE;
using Xunit;

namespace Test.ORESchemes
{
	[Trait("Category", "Unit")]
	public class LewiORETests : GenericORETests<Ciphertext>
	{
		protected override void SetScheme()
		{
			_scheme = new LewiOREScheme(_entropy);
		}
	}
}
