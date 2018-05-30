using System;
using System.Text;
using ORESchemes.Shared.Primitives;
using Xunit;

namespace Test.ORESchemes.Primitives
{
	[Trait("Category", "Unit")]
	public class AESPRFTests : AbsPRFTests
	{
		public AESPRFTests() : base(new AES()) { }

		[Theory]
		[InlineData("Hello")]
		[InlineData("World")]
		[InlineData("")]
		[InlineData("1305")]
		public void StringCorrectness(string plaintext)
		{
			var ciphertext = _prf.PRF(_key, Encoding.Default.GetBytes(plaintext));

			var decrypted = _prf.InversePRF(_key, ciphertext);

			Assert.Equal(plaintext, Encoding.Default.GetString(decrypted));
		}
	}

	[Trait("Category", "Unit")]
	public class FeistelPRFTests : AbsPRFTests
	{
		public FeistelPRFTests() : base(new Feistel(3)) { }
	}

	[Trait("Category", "Unit")]
	public class FeistelStrongPRFTests : AbsPRFTests
	{
		public FeistelStrongPRFTests() : base(new Feistel(4)) { }
	}

	public abstract class AbsPRFTests
	{
		protected readonly IPRF _prf;
		private const int SEED = 123456;
		protected readonly byte[] _key = new byte[256 / 8];
		private const int RUNS = 100000;

		public AbsPRFTests(IPRF prf)
		{
			new Random(SEED).NextBytes(_key);
			_prf = prf;
		}

		[Fact]
		public void CorrectnessIntTest()
		{
			for (int i = -RUNS; i < RUNS; i++)
			{
				byte[] encrypted = _prf.PRF(_key, BitConverter.GetBytes(i));
				int decrypted = BitConverter.ToInt32(_prf.InversePRF(_key, encrypted), 0);
				Assert.Equal(i, decrypted);
			}
		}

		[Fact]
		public void CorrectnessTest()
		{
			Random random = new Random(SEED);

			for (int i = 0; i < RUNS; i++)
			{
				byte[] plaintext = new byte[4];
				random.NextBytes(plaintext);

				Assert.Equal(
					plaintext,
					_prf.InversePRF(
						_key,
						_prf.PRF(_key, plaintext)
					)
				);
			}
		}
	}
}
