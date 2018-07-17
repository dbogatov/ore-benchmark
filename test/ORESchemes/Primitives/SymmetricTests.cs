using System;
using System.Collections.Generic;
using ORESchemes.Shared.Primitives.Symmetric;
using Xunit;

namespace Test.ORESchemes.Primitives.Symmetric
{
	[Trait("Category", "Unit")]
	public class AESChecks
	{
		private const int RUNS = 1000;
		private const int SEED = 123465;
		private readonly Random G = new Random(SEED);
		private readonly byte[] _key = new byte[128 / 8];
		private readonly AESSymmetric aes = new AESSymmetric();

		public AESChecks() => G.NextBytes(_key);

		[Theory]
		[InlineData(0.5)]
		[InlineData(1)]
		[InlineData(2)]
		public void Correctness(double size)
		{
			byte[] input = new byte[Convert.ToInt32(size * 128 / 8)];

			for (int i = 0; i < RUNS; i++)
			{
				G.NextBytes(input);
				var encrypted = aes.Encrypt(_key, input);
				var decrypted = aes.Decrypt(_key, encrypted);

				Assert.Equal(input, decrypted);
			}
		}

		[Fact]
		public void Randomization()
		{
			byte[] input = new byte[2 * 128 / 8];

			HashSet<byte[]> set = new HashSet<byte[]>();
			for (int i = 0; i < RUNS; i++)
			{
				G.NextBytes(input);
				set.Add(aes.Encrypt(_key, input));
			}

			Assert.Equal(RUNS, set.Count);
		}
	}
}
