using System;
using System.Text;
using ORESchemes.Shared.Primitives;
using Xunit;

namespace Test.ORESchemes
{
	public class PrimitiveTests
	{
		[Theory]
		[InlineData("Hello")]
		[InlineData("World")]
		[InlineData("")]
		[InlineData("1305")]
		public void AESStringCorrectness(string plaintext)
		{
			var seed = 245613;
			var random = new Random(seed);

			AES aes = new AES();

			aes.SetSecurityParameter(256);
			byte[] key = new byte[256 / 8];
			random.NextBytes(key);

			Assert.Equal(
				plaintext,
				Encoding
					.Default
					.GetString(
						aes
							.InversePRF(
								key,
								aes
									.PRF(
										key,
										Encoding
											.Default
											.GetBytes(plaintext)
										)
							)
					)
			);
		}

		[Fact]
		public void AESIntCorrectness()
		{
			var seed = 245613;
			var random = new Random(seed);

			AES aes = new AES();

			aes.SetSecurityParameter(256);
			byte[] key = new byte[256 / 8];
			random.NextBytes(key);

			for (int i = 0; i < 100; i++)
			{
				var plaintext = random.Next();

				Assert.Equal(
					plaintext,
					BitConverter
						.ToInt32(
							aes
								.InversePRF(
									key,
									aes
										.PRF(
											key,
											BitConverter
												.GetBytes(plaintext)
										)
								),
							0
					)
				);
			}

		}

		[Theory]
		[InlineData(128)]
		[InlineData(256)]
		public void AESCorrectnessTest(int alpha)
		{
			var seed = 245613;
			var random = new Random(seed);

			AES aes = new AES();

			aes.SetSecurityParameter(alpha);
			byte[] key = new byte[alpha / 8];
			random.NextBytes(key);

			for (int i = 0; i < 1000; i++)
			{
				byte[] plaintext = new byte[4];
				random.NextBytes(plaintext);

				Assert.Equal(
					plaintext,
					aes.InversePRF(
						key,
						aes.PRF(key, plaintext)
					)
				);
			}
		}
	}
}
