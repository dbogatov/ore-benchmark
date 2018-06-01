using System;
using ORESchemes.LewiORE;
using Xunit;

namespace Test.ORESchemes
{
	// [Trait("Category", "Unit")]
	// public class LewiORETests : GenericORETests<Ciphertext>
	// {
	// 	protected override void SetScheme()
	// 	{
	// 		_scheme = new LewiOREScheme(_entropy);
	// 	}
	// }

	// public class Lewi
	// {
	// 	[Fact]
	// 	public void Correctness()
	// 	{
	// 		var entropy = new byte[256 / 8];
	// 		new Random(123456).NextBytes(entropy);
	// 		var _scheme = new LewiOREScheme(entropy);

	// 		_scheme.Init();

	// 		var key = _scheme.KeyGen();

	// 		var c1 = _scheme.Encrypt(1935032234, key);
	// 		var c2 = _scheme.Encrypt(1935032235, key);

	// 		var r = _scheme.IsLess(c1, c2);

	// 		Assert.True(r);
	// 	}
	// }
}
