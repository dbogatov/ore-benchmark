using System;
using System.Collections.Generic;
using System.Linq;
using ORESchemes.FHOPE;
using ORESchemes.Shared;
using ORESchemes.Shared.Primitives;
using Xunit;

namespace Test.ORESchemes
{
	[Trait("Category", "Unit")]
	public class FHOPETests : GenericORETests<Ciphertext, State>
	{
		protected override void SetScheme()
		{
			_scheme = new FHOPEScheme(long.MinValue, long.MaxValue, _entropy);

			_expectedEvents = new Dictionary<SchemeOperation, Tuple<int, int>>
			{
				{ SchemeOperation.Init, new Tuple<int, int>(1, 1)} ,
				{ SchemeOperation.KeyGen, new Tuple<int, int>(1, 1) },
				{ SchemeOperation.Destruct, new Tuple<int, int>(1, 1) },
				{ SchemeOperation.Encrypt, new Tuple<int, int>(10, 15) },
				{ SchemeOperation.Decrypt, new Tuple<int, int>(10, 100) },
				{ SchemeOperation.Comparison, new Tuple<int, int>(9 * 5, 9 * 5 * 4) },
			};
		}

		protected override Ciphertext ConfigureCiphertext(Ciphertext cipher, State key)
		{
			FHOPEScheme scheme = Assert.IsType<FHOPEScheme>(_scheme);

			var plaintext = scheme.Decrypt(cipher, key);

			cipher.max = scheme.MaxCiphertext(plaintext, key);
			cipher.min = scheme.MinCiphertext(plaintext, key);

			return cipher;
		}

		[Fact]
		public override void KeyGenTest()
		{
			_scheme.Init();
			var key = _scheme.KeyGen();

			Assert.NotNull(key);
		}

		[Fact]
		public void ExceptionsTest()
		{
			FHOPEScheme scheme = Assert.IsType<FHOPEScheme>(_scheme);

			var key = scheme.KeyGen();

			Assert.Throws<InvalidOperationException>(
				() => scheme.MaxCiphertext(100, key)
			);

			Assert.Throws<InvalidOperationException>(
				() => scheme.MinCiphertext(100, key)
			);

			Assert.Throws<InvalidOperationException>(
				() => scheme.Decrypt(new Ciphertext { value = 100 }, key)
			);
		}

		#pragma warning disable xUnit1026
		[Theory(Skip = "Not applicable")]
		[InlineData(-10, -10)]
		public override void OrderCorrectnessTest(int plaintextOne, int plaintextTwo) {}

		[Theory]
		[InlineData(0, 0)]
		[InlineData(1, 1)]
		[InlineData(-1, -1)]
		[InlineData(-1, 1)]
		[InlineData(1, -1)]
		[InlineData(2, 1)]
		[InlineData(1, 2)]
		[InlineData(-2, -1)]
		[InlineData(-1, -2)]
		public void FHOPEOrderCorrectnessTest(int plaintextOne, int plaintextTwo)
		{
			FHOPEScheme scheme = Assert.IsType<FHOPEScheme>(_scheme);

			scheme.Init();

			var key = scheme.KeyGen();

			for (int i = 0; i < _runs; i++)
			{
				do
				{
					bool mutation = false;

					var pOne = plaintextOne * (i + 1);
					var pTwo = plaintextTwo * (i + 1);

					key.MutationOcurred += new MutationEventHandler(() => mutation = true);

					var ciphertextOne = scheme.Encrypt(pOne, key);
					var ciphertextTwo = scheme.Encrypt(pTwo, key);

					if (mutation)
					{
						continue;
					}

					ciphertextOne = ConfigureCiphertext(ciphertextOne, key);
					ciphertextTwo = ConfigureCiphertext(ciphertextTwo, key);

					Assert.Equal(pOne > pTwo, scheme.IsGreater(ciphertextOne, ciphertextTwo));
					Assert.Equal(pOne < pTwo, scheme.IsLess(ciphertextOne, ciphertextTwo));
					Assert.Equal(pOne >= pTwo, scheme.IsGreaterOrEqual(ciphertextOne, ciphertextTwo));
					Assert.Equal(pOne <= pTwo, scheme.IsLessOrEqual(ciphertextOne, ciphertextTwo));
					Assert.Equal(pOne == pTwo, scheme.IsEqual(ciphertextOne, ciphertextTwo));
				} while (false);
			}
		}
	}
}
