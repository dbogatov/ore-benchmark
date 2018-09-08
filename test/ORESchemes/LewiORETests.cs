using System;
using ORESchemes.LewiORE;
using Xunit;

namespace Test.ORESchemes
{
	[Trait("Category", "Unit")]
	public class LewiOREN16 : AbsLewiORE
	{
		public LewiOREN16() : base(100) { }

		protected override void SetParameters() => n = 16;

		public override int CipherSize() => 2816;
	}

	[Trait("Category", "Integration")]
	public class LewiOREN8 : AbsLewiORE
	{
		public LewiOREN8() : base(50) { }

		protected override void SetParameters() => n = 8;

		public override int CipherSize() => 1664;
	}

	[Trait("Category", "Integration")]
	public class LewiOREN4 : AbsLewiORE
	{
		public LewiOREN4() : base(30) { }

		protected override void SetParameters() => n = 4;

		public override int CipherSize() => 2816;
	}

	[Trait("Category", "Unit")]
	public class LewiORENMalformed
	{
		[Theory]
		[InlineData(1)]
		[InlineData(2)]
		[InlineData(3)]
		[InlineData(32)]
		public void MalformedN(int n)
			=> Assert.Throws<ArgumentException>(
				() => new LewiOREScheme(n)
			);
	}

	public abstract class AbsLewiORE : GenericORE<Ciphertext, Key>
	{
		protected int n = 16;

		public AbsLewiORE(int runs) : base(runs) { }

		protected override void SetScheme()
		{
			_scheme = new LewiOREScheme(n, _entropy);
		}

		public override int KeySize() => 256;
	}
}
