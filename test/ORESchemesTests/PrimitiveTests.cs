using System;
using System.Text;
using ORESchemes.Shared.Primitives;
using Xunit;

namespace Test.ORESchemes
{
	public class PrimitiveTests
	{
		// [Theory]
		// [InlineData("Hello")]
		// public void AESStringCorrectness(string plaintext)
		// {
		// 	var seed = 245613;
		// 	var random = new Random(seed);

		// 	IPRF aes = PRFFactory.GetPRF();

		// 	aes.SetSecurityParameter(128);
		// 	byte[] key = new byte[128 / 8];
		// 	random.NextBytes(key);

		// 	var ciphertext = aes.PRF(key, Encoding.UTF8.GetBytes(plaintext));
		// 	var decrypted = aes.InversePRF(key, ciphertext);

		// 	Assert.Equal(plaintext, Convert.ToBase64String(decrypted));
		// }

		[Theory]
		[InlineData(128)]
		[InlineData(256)]
		public void AESCorrectnessTest(int alpha)
		{
			var seed = 245613;

			var random = new Random(seed);
			IPRF aes = PRFFactory.GetPRF();

			aes.SetSecurityParameter(alpha);
			byte[] key = new byte[alpha / 8];
			random.NextBytes(key);

			for (int i = 0; i < 1000; i++)
			{
				byte[] plaintext = new byte[4];
				random.NextBytes(plaintext);

				var ciphertext = aes.PRF(key, plaintext);
				var decrypted = aes.InversePRF(key, ciphertext);

				Assert.Equal(plaintext, decrypted);
			}
		}
	}
}
