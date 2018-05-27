
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
		[Fact]
		public void UniformityTest()
		{
			var values = new Dictionary<ulong, int>(RUNS);
			for (int i = 0; i < RUNS * 100; i++)
			{
				var value = _sampler.Uniform(0, RUNS);

				if (values.ContainsKey(value))
				{
					values[value]++;
				}
				else
				{
					values.Add(value, 1);
				}
			}

			var stdDev = values.Values.StdDev();

			Assert.InRange(values.Values.StdDev(), 0, RUNS * 0.02);
			Assert.InRange(values.Where(kvp => kvp.Key < 100).Select(kvp => kvp.Value).StdDev(), 0, RUNS * 0.02);
			Assert.InRange(values.Where(kvp => kvp.Key > RUNS - 100).Select(kvp => kvp.Value).StdDev(), 0, RUNS * 0.02);
			Assert.InRange(
				values.Where(kvp => kvp.Key > RUNS / 2 - 50 && kvp.Key < RUNS / 2 + 50).Select(kvp => kvp.Value).StdDev(),
				0, RUNS * 0.02
			);
		}

		[Fact]
		public void RangesTest()
		{
			Random random = new Random(SEED);

			for (int i = 0; i < RUNS; i++)
			{
				var a = (ulong)(random.NextDouble() * UInt64.MaxValue);
				var b = (ulong)(random.NextDouble() * UInt64.MaxValue);

				if (a == b)
				{
					continue;
				}

				var min = Math.Min(a, b);
				var max = Math.Max(a, b);

				var value = _sampler.Uniform(min, max);

				Assert.InRange(value, min, max);
			}
		}
	}
}
