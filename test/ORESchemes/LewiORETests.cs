using System;
using ORESchemes.LewiORE;
using Xunit;

namespace Test.ORESchemes
{
	[Trait("Category", "Unit")]
	public class LewiORETestsN16 : LewiORETests
	{
		public LewiORETestsN16() : base(100) { }

		protected override void SetParameters() => _n = 16;
	}

	[Trait("Category", "Unit")]
	public class LewiORETestsN8 : LewiORETests
	{
		public LewiORETestsN8() : base(50) { }

		protected override void SetParameters() => _n = 8;
	}

	[Trait("Category", "Unit")]
	public class LewiORETestsN4 : LewiORETests
	{
		public LewiORETestsN4() : base(30) { }

		protected override void SetParameters() => _n = 4;
	}

	[Trait("Category", "Unit")]
	public class LewiORETestsNMalformed
	{
		[Theory]
		[InlineData(1)]
		[InlineData(2)]
		[InlineData(3)]
		[InlineData(32)]
		public void MalformedNTest(int n)
			=> Assert.Throws<ArgumentException>(
				() => new LewiOREScheme(n)
			);
	}

	public abstract class LewiORETests : GenericORETests<Ciphertext>
	{
		protected int _n;

		public LewiORETests(int runs) : base(runs) { }

		protected override void SetScheme()
		{
			_scheme = new LewiOREScheme(_n, _entropy);
		}
	}
}
