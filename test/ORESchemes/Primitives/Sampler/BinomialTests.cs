
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
		[InlineData(40, 0.5, 0.03)]
		[InlineData(20, 0.7, 0.03)]
		[InlineData(20, 0.5, 0.03)]
		public void BinomialDistributionTest(int n, double p, double epsilon)
		{
			Func<int, double> pmf =
				(k) =>
					k < 0 || k > n ? 0 :
					SpecialFunctions.Binomial(n, k) *
					Math.Pow(p, k) *
					Math.Pow(1 - p, n - k);

			var values = new Dictionary<ulong, int>(RUNS);
			for (int i = 0; i < RUNS; i++)
			{
				var value = _sampler.Binomial((ulong)n, p);

				if (values.ContainsKey(value))
				{
					values[value]++;
				}
				else
				{
					values.Add(value, 1);
				}
			}

			for (int k = 0; k <= n; k++)
			{
				var actual = (double)(values.ContainsKey((ulong)k) ? values[(ulong)k] : 0) / RUNS;
				var expected = pmf(k);

				// Console.WriteLine($"{k}: {actual.ToString("0.00")} vs {expected.ToString("0.00")}");
				Assert.InRange(actual, expected - epsilon, expected + epsilon);
			}
		}

		[Fact]
		public void BinomialLargeInputs()
		{
			_sampler.Binomial((ulong)Int32.MaxValue, 0.1 / 100000);
		}
	}
}
