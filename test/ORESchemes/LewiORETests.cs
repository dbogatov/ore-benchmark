using System;
using ORESchemes.LewiORE;
using Xunit;

namespace Test.ORESchemes
{
	[Trait("Category", "Unit")]
	public class LewiORETestsN16 : AbsLewiORETests
	{
		public LewiORETestsN16() : base(100) { }

		protected override void SetParameters() => n = 16;

		public override int CipherSize() => 4368;
	}

	[Trait("Category", "Integration")]
	public class LewiORETestsN8 : AbsLewiORETests
	{
		public LewiORETestsN8() : base(50) { }

		protected override void SetParameters() => n = 8;

		public override int CipherSize() => 2440;
	}

	[Trait("Category", "Integration")]
	public class LewiORETestsN4 : AbsLewiORETests
	{
		public LewiORETestsN4() : base(30) { }

		protected override void SetParameters() => n = 4;

		public override int CipherSize() => 3204;
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

	public abstract class AbsLewiORETests : GenericORETests<Ciphertext, Key>
	{
		protected int n = 16;

		public AbsLewiORETests(int runs) : base(runs) { }

		protected override void SetScheme()
		{
			_scheme = new LewiOREScheme(n, _entropy);
		}

		public override int KeySize() => 256;
	}
}
