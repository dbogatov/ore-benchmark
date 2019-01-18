using System;
using Crypto.LewiWu;
using Xunit;

namespace Test.Crypto
{
	[Trait("Category", "Unit")]
	public class LewiWuN16 : AbsLewiWu
	{
		public LewiWuN16() : base(100) { }

		protected override void SetParameters() => n = 16;

		public override int CipherSize() => 2816;
	}

	[Trait("Category", "Integration")]
	public class LewiWuN8 : AbsLewiWu
	{
		public LewiWuN8() : base(50) { }

		protected override void SetParameters() => n = 8;

		public override int CipherSize() => 1664;
	}

	[Trait("Category", "Integration")]
	public class LewiWuN4 : AbsLewiWu
	{
		public LewiWuN4() : base(10) { }

		protected override void SetParameters() => n = 4;

		public override int CipherSize() => 2816;
	}

	[Trait("Category", "Unit")]
	public class LewiWuNMalformed
	{
		[Theory]
		[InlineData(1)]
		[InlineData(2)]
		[InlineData(3)]
		[InlineData(32)]
		public void MalformedN(int n)
			=> Assert.Throws<ArgumentException>(
				() => new Scheme(n)
			);
	}

	public abstract class AbsLewiWu : GenericORE<Ciphertext, Key>
	{
		protected int n = 16;

		public AbsLewiWu(int runs) : base(runs) { }

		protected override void SetScheme()
		{
			_scheme = new Scheme(n, _entropy);
		}

		public override int KeySize() => 256;

		[Fact]
		public void InvertedNulls()
		{
			var key = _scheme.KeyGen();

			var ciphertextOne = _scheme.Encrypt(10, key);
			var ciphertextTwo = _scheme.Encrypt(15, key);

			ciphertextOne.left = null;
			ciphertextTwo.right = null;

			Assert.False(_scheme.IsEqual(ciphertextOne, ciphertextTwo));
			Assert.True(_scheme.IsLess(ciphertextOne, ciphertextTwo));
			Assert.True(_scheme.IsLessOrEqual(ciphertextOne, ciphertextTwo));
			Assert.False(_scheme.IsGreater(ciphertextOne, ciphertextTwo));
			Assert.False(_scheme.IsGreaterOrEqual(ciphertextOne, ciphertextTwo));
		}

		[Fact]
		public void RightLeftViolation()
		{
			Assert.Throws<InvalidOperationException>(
				() =>
				{
					var key = _scheme.KeyGen();

					var ciphertextOne = _scheme.Encrypt(10, key);
					var ciphertextTwo = _scheme.Encrypt(15, key);

					ciphertextOne.left = null;
					ciphertextTwo.left = null;

					_scheme.IsEqual(ciphertextOne, ciphertextTwo);
				}
			);
		}
	}
}
