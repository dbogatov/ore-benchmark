using System;
using ORESchemes.PracticalORE;

namespace Test.ORESchemes
{
	public class PracticalORETests : GenericORETests<Ciphertext>
	{
		protected override void SetScheme()
		{
			_scheme = new PracticalOREScheme(BitConverter.GetBytes(123456));
		}
	}
}
