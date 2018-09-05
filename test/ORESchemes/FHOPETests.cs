using System;
using System.Collections;
using System.Collections.Generic;
using ORESchemes.FHOPE;
using ORESchemes.Shared;
using Xunit;

namespace Test.ORESchemes
{
	[Trait("Category", "Unit")]
	public class PerfectFHOPE : AbsFHOPE
	{
		public override double GetP() => 0;
	}

	[Trait("Category", "Unit")]
	public class ImperfectFHOPE : AbsFHOPE
	{
		public override double GetP() => 0.5;

		[Fact]
		public void PerfectVsImperfect()
		{
			Random random = new Random(SEED);

			var key = _scheme.KeyGen();

			var perfect = new FHOPEScheme(long.MinValue, long.MaxValue, 10, 0, _entropy);
			var pKey = perfect.KeyGen();

			for (int i = 0; i < _runs; i++)
			{
				var plaintext = random.Next(0, _runs / 10);
				_scheme.Encrypt(plaintext, key);
				perfect.Encrypt(plaintext, pKey);
			}

			Assert.True(key.GetSize() < pKey.GetSize());
		}
	}

	public abstract class AbsFHOPE : GenericORE<Ciphertext, State>
	{
		public abstract double GetP();

		protected override void SetScheme()
		{
			_scheme = new FHOPEScheme(long.MinValue, long.MaxValue, 10, GetP(), _entropy);

			_expectedEvents = new Dictionary<SchemeOperation, Tuple<int, int>>
			{
				{ SchemeOperation.KeyGen, new Tuple<int, int>(1, 1) },
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
		public override void KeyGen()
		{
			var key = _scheme.KeyGen();

			Assert.NotNull(key);
		}

		[Fact]
		public void Exceptions()
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
		public override void OrderCorrectness(int plaintextOne, int plaintextTwo) { }

		[Theory]
		[ClassData(typeof(OrderCorrectnessTestData))]
		public void FHOPEOrderCorrectness(int plaintextOne, int plaintextTwo, bool configureFirst)
		{
			FHOPEScheme scheme = Assert.IsType<FHOPEScheme>(_scheme);

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

					if (configureFirst)
					{
						ciphertextOne = ConfigureCiphertext(ciphertextOne, key);
					}
					else
					{
						ciphertextTwo = ConfigureCiphertext(ciphertextTwo, key);
					}

					Assert.Equal(pOne > pTwo, scheme.IsGreater(ciphertextOne, ciphertextTwo));
					Assert.Equal(pOne < pTwo, scheme.IsLess(ciphertextOne, ciphertextTwo));
					Assert.Equal(pOne >= pTwo, scheme.IsGreaterOrEqual(ciphertextOne, ciphertextTwo));
					Assert.Equal(pOne <= pTwo, scheme.IsLessOrEqual(ciphertextOne, ciphertextTwo));
					Assert.Equal(pOne == pTwo, scheme.IsEqual(ciphertextOne, ciphertextTwo));

				} while (false);
			}
		}

		private class OrderCorrectnessTestData : IEnumerable<object[]>
		{
			public IEnumerator<object[]> GetEnumerator()
			{
				for (int i = -2; i <= 2; i++)
				{
					for (int j = -2; j <= 2; j++)
					{
						for (int k = 0; k < 2; k++)
						{
							if (i * j != 0 && Math.Abs(i) != Math.Abs(j))
							{
								yield return new object[] { i, j, k == 0 };
							}
						}
					}
				}
			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}

		public override int KeySize() => 0;
		public override int CipherSize() => 24;
	}
}
