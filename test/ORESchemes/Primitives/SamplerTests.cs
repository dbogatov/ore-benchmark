using System;
using System.Collections.Generic;
using ORESchemes.Shared.Primitives;
using ORESchemes.Shared;
using Xunit;
using System.Linq;

namespace Test.ORESchemes.Primitives
{
	public class CustomSamplerTests
	{
		private const int SEED = 123456;
		private readonly byte[] _entropy = new byte[256 / 8];

		private const int RUNS = 1000;

		public CustomSamplerTests()
		{
			new Random(SEED).NextBytes(_entropy);
		}

		[Fact]
		public void UniformityTest()
		{
			ISampler<long, ulong> sampler = new CustomSampler(_entropy);

			var values = new Dictionary<long, int>(RUNS);
			for (int i = 0; i < RUNS * 100; i++)
			{
				var value = sampler.Uniform(0, RUNS);

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
			ISampler<long, ulong> sampler = new CustomSampler(_entropy);
			Random random = new Random(SEED);

			for (int i = 0; i < RUNS; i++)
			{
				var a = (long)(random.NextDouble() * Int64.MaxValue);
				var b = (long)(random.NextDouble() * Int64.MaxValue);

				if (a == b)
				{
					continue;
				}

				var min = Math.Min(a, b);
				var max = Math.Max(a, b);

				var value = sampler.Uniform(min, max);

				Assert.InRange(value, min, max);
			}
		}

		[Fact]
		public void HGDistributionTest()
		{
			ISampler<long, ulong> sampler = new CustomSampler(_entropy);

			var values = new Dictionary<ulong, int>(RUNS);
			for (int i = 0; i < RUNS; i++)
			{
				var value = sampler.HyperGeometric(500, 60, 200);

				// Console.WriteLine(value);

				if (values.ContainsKey(value))
				{
					values[value]++;
				}
				else
				{
					values.Add(value, 1);
				}
			}

			// // Console.WriteLine("Hello");

			for (ulong i = 0; i < 60; i++)
			{
				var p = (double)(values.ContainsKey(i) ? values[i] : 0) / RUNS;
				Console.WriteLine($"{i}: {p.ToString("0.00")}");
			}

			// var value = new CustomSampler(_entropy).HyperGeometric(UInt64.MaxValue, UInt64.MaxValue / 2, (ulong)UInt32.MaxValue);
		}
	}
}
