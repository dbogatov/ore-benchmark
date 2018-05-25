using System;
using System.Text;
using ORESchemes.Shared.Primitives;
using ORESchemes.Shared;
using Xunit;
using System.Collections.Generic;
using System.Linq;

namespace Test.ORESchemes.Primitives
{
	public class AESPRGTests : PRGTests
	{
		public AESPRGTests() : base()
		{
			_prg = new AESPRG(_entropy);
			_anotherPrg = new AESPRG(_anotherEntropy);
		}
	}

	public class DefaultRandomTests : PRGTests
	{
		public DefaultRandomTests() : base()
		{
			_prg = new DefaultRandom(_entropy);
			_anotherPrg = new DefaultRandom(_anotherEntropy);
		}
	}

	public abstract class PRGTests
	{
		protected const int _seed = 132456;
		protected readonly byte[] _entropy = new byte[256 / 8];
		protected readonly byte[] _anotherEntropy = new byte[256 / 8];

		protected const int _runs = 1000;

		protected IPRG _prg;
		protected IPRG _anotherPrg;

		public PRGTests()
		{
			new Random(_seed).NextBytes(_entropy);
			new Random(_seed + 1).NextBytes(_anotherEntropy);
		}

		[Fact]
		public void NoExceptionsTest()
		{
			const int runs = 10;

			for (int i = 0; i < runs; i++)
			{
				var value = _prg.Next();
				// Console.WriteLine($"Integer: {value.ToString().PadLeft(15)}");
			}

			for (int i = 0; i < runs; i++)
			{
				var value = _prg.NextLong();
				// Console.WriteLine($"Long: {value.ToString().PadLeft(20)}");
			}

			for (int i = 0; i < runs; i++)
			{
				byte[] bytes = new byte[20];
				_prg.NextBytes(bytes);
				// Console.WriteLine($"Bytes: {bytes.Print()}");
			}
		}

		[Theory]
		[InlineData(1)]
		[InlineData(2)]
		[InlineData(4)]
		[InlineData(8)]
		[InlineData(16)]
		[InlineData(128)]
		[InlineData(256)]
		[InlineData(512)]
		[InlineData(1024)]
		public void DifferentSizeRequestsTest(int size)
		{
			byte[] bytes = new byte[size];
			_prg.NextBytes(bytes);

			Assert.False(bytes.All(b => b == 0x00));
		}

		[Fact]
		public void NoRepetitionsTest()
		{
			var values = new HashSet<int>(_runs);
			for (int i = 0; i < _runs; i++)
			{
				values.Add(_prg.Next());
			}

			Assert.Equal(_runs, values.Count);
		}

		[Fact]
		public void DifferentSeedsTest()
		{
			var values = new HashSet<int>(_runs);
			for (int i = 0; i < _runs; i++)
			{
				values.Add(_prg.Next());
				values.Add(_anotherPrg.Next());
			}

			Assert.Equal(_runs * 2, values.Count);
		}

		[Fact]
		public void UniformityTest()
		{
			var values = new Dictionary<int, int>(_runs);
			for (int i = 0; i < _runs * 100; i++)
			{
				var value = _prg.Next(_runs);

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

			Assert.InRange(values.Values.StdDev(), 0, _runs * 0.02);
			Assert.InRange(values.Where(kvp => kvp.Key < 100).Select(kvp => kvp.Value).StdDev(), 0, _runs * 0.02);
			Assert.InRange(values.Where(kvp => kvp.Key > _runs - 100).Select(kvp => kvp.Value).StdDev(), 0, _runs * 0.02);
			Assert.InRange(
				values.Where(kvp => kvp.Key > _runs / 2 - 50 && kvp.Key < _runs / 2 + 50).Select(kvp => kvp.Value).StdDev(),
				0, _runs * 0.02
			);
		}

		[Fact]
		public void RangesIntTest()
		{
			var random = new Random(_seed);
			CheckRanges<int>(random.Next, _prg.Next);
		}

		[Fact]
		public void RangesLongTest()
		{
			var random = new Random(_seed);
			CheckRanges<long>(() => (long)(random.NextDouble() * Int64.MaxValue), _prg.NextLong);
		}

		[Fact]
		public void RangesDoubleTest()
		{
			var random = new Random(_seed);
			CheckRanges<double>(random.NextDouble, _prg.NextDouble);
		}

		private void CheckRanges<T>(Func<T> nextRand, Func<T, T, T> nextPrg) where T : IComparable
		{
			for (int i = 0; i < _runs; i++)
			{
				var a = nextRand();
				var b = nextRand();

				T min, max;
				if (a.CompareTo(b) < 0)
				{
					min = a;
					max = b;
				}
				else if (a.CompareTo(b) > 0)
				{
					min = b;
					max = a;
				}
				else
				{
					continue;
				}

				var result = nextPrg(min, max);
				Assert.InRange(result, min, max);
			}
		}
	}
}
