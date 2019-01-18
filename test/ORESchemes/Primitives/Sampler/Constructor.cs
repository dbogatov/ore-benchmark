using System;
using Xunit;
using Crypto.Shared.Primitives.Sampler;

namespace Test.Crypto.Primitives
{
	[Trait("Category", "Unit")]
	public partial class Sampler
	{
		private const int SEED = 123456;
		private readonly byte[] _entropy = new byte[128 / 8];
		private readonly ISampler _sampler;

		private const int RUNS = 1000;

		public Sampler()
		{
			new Random(SEED).NextBytes(_entropy);
			_sampler = new CustomSampler(_entropy);
		}
	}
}
