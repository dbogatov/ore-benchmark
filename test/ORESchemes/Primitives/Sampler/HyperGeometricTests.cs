
using System;
using System.Collections.Generic;
using ORESchemes.Shared.Primitives;
using ORESchemes.Shared;
using Xunit;
using System.Linq;
using MathNet.Numerics;

namespace Test.ORESchemes.Primitives
{
	public partial class SamplerTests
	{
		[Theory]
		[InlineData(99, 10, 25, 0.05)]
		[InlineData(500, 50, 100, 0.03)]
		[InlineData(500, 60, 200, 0.02)]
		[InlineData(500, 70, 300, 0.01)]
		public void HGDistributionTest(int N, int K, int n, double epsilon)
		{
			Func<int, double> pmf =
				(k) =>
					k < Math.Max(0, n + K - N) || k > Math.Min(n, K) ? 0 :
					SpecialFunctions.Binomial(K, k) *
					SpecialFunctions.Binomial(N - K, n - k) /
					SpecialFunctions.Binomial(N, n);

			var values = new Dictionary<ulong, int>(RUNS);
			for (int i = 0; i < RUNS; i++)
			{
				var value = _sampler.HyperGeometric((ulong)N, (ulong)K, (ulong)n);

				if (values.ContainsKey(value))
				{
					values[value]++;
				}
				else
				{
					values.Add(value, 1);
				}
			}

			for (int k = Math.Max(0, n + K - N); k <= Math.Min(n, K); k++)
			{
				var actual = (double)(values.ContainsKey((ulong)k) ? values[(ulong)k] : 0) / RUNS;
				var expected = pmf(k);

				Assert.InRange(actual, expected - epsilon, expected + epsilon);
			}
		}

		[Fact]
		public void HGLargeInputsTest()
		{
			_sampler.HyperGeometric(Int64.MaxValue / 100, (ulong)UInt32.MaxValue, (ulong)UInt32.MaxValue);
		}

		[Fact]
		/// <summary>
		/// These inputs are know to cause trouble
		/// </summary>
		public void HGInconvenientInputsTest()
		{
			_sampler.HyperGeometric(144115188075855871, 72057594037927936, 33562748);
			_sampler.HyperGeometric(72057594037927935, 36028797018963968, 16781410);
			_sampler.HyperGeometric(36028797018963967, 18014398509481984, 8392237);

			_sampler.HyperGeometric(18014398509481983 / 2, 9007199254740992 / 2, 4196122 / 2);

			_sampler.HyperGeometric(18014398509481983, 9007199254740992, 4196122);
		}

		[Theory]
		[InlineData(99, 10, 25)]
		[InlineData(500, 50, 100)]
		[InlineData(500, 60, 200)]
		[InlineData(500, 70, 300)]
		public void HGCorrectnessTest(int N, int K, int n)
		{
			Random random = new Random(SEED);

			for (int i = 0; i < RUNS * 10; i++)
			{
				var value = _sampler.HyperGeometric((ulong)N, (ulong)K, (ulong)n);

				Assert.InRange((int)value, Math.Max(0, n + K - N), Math.Min(n, K));
			}
		}
	}
}
