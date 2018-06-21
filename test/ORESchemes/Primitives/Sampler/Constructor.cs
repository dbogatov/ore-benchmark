using System;
using Xunit;
using ORESchemes.Shared.Primitives.Sampler;

namespace Test.ORESchemes.Primitives
{
	[Trait("Category", "Unit")]
	public partial class SamplerTests
	{
		private const int SEED = 123456;
		private readonly byte[] _entropy = new byte[256 / 8];
		private readonly ISampler _sampler;

		private const int RUNS = 1000;

		public SamplerTests()
		{
			new Random(SEED).NextBytes(_entropy);
			_sampler = new CustomSampler(_entropy);
		}
	}
}
