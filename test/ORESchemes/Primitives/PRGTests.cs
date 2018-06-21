using System;
using ORESchemes.Shared.Primitives;
using ORESchemes.Shared;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using ORESchemes.Shared.Primitives.PRG;

namespace Test.ORESchemes.Primitives.PRG
{
	[Trait("Category", "Unit")]
	public class AESPRGTests : AbsPRGTests
	{
		public AESPRGTests() : base()
		{
			_prg = new AESPRG(_entropy);
			_anotherPrg = new AESPRG(_anotherEntropy);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void FactoryTest(bool seed)
		{
			byte[] entropy = seed ? _entropy : null;

			var prg = PRGFactory.GetPRG(entropy);

			Assert.NotNull(prg);
			Assert.IsType<AESPRG>(prg);
		}
	}

	[Trait("Category", "Unit")]
	public class DefaultRandomTests : AbsPRGTests
	{
		public DefaultRandomTests() : base()
		{
			_prg = new DefaultRandom(_entropy);
			_anotherPrg = new DefaultRandom(_anotherEntropy);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void FactoryTest(bool seed)
		{
			byte[] entropy = seed ? _entropy : null;

			var prg = PRGFactory.GetDefaultPRG(entropy);

			Assert.NotNull(prg);
			Assert.IsType<DefaultRandom>(prg);
		}
	}

	public abstract class AbsPRGTests
	{
		protected const int _seed = 132456;
		protected readonly byte[] _entropy = new byte[256 / 8];
		protected readonly byte[] _anotherEntropy = new byte[256 / 8];

		protected const int _runs = 1000;

		protected IPRG _prg;
		protected IPRG _anotherPrg;

		public AbsPRGTests()
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
			}

			for (int i = 0; i < runs; i++)
			{
				var value = _prg.NextLong();
			}

			for (int i = 0; i < runs; i++)
			{
				byte[] bytes = new byte[20];
				_prg.NextBytes(bytes);
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
		public void NextDoubleTest()
		{
			HashSet<double> set = new HashSet<double>();

			for (int i = 0; i < _runs; i++)
			{
				set.Add(_prg.NextDouble());
			}

			Assert.Equal(_runs, set.Count);
		}

		[Fact]
		public void RangesIntTest()
		{
			var random = new Random(_seed);
			CheckRanges<int>(random.Next, _prg.Next);

			CheckMax<int>(random.Next, _prg.Next, 0);
		}

		[Fact]
		public void RangesLongTest()
		{
			var random = new Random(_seed);
			CheckRanges<long>((a, b) => (long)(random.NextDouble() * Int64.MaxValue), _prg.NextLong);

			CheckMax<long>(a => (long)(random.NextDouble() * Int64.MaxValue), _prg.NextLong, 0);
		}

		[Fact]
		public void RangesDoubleTest()
		{
			var random = new Random(_seed);
			CheckRanges<double>((a, b) => random.NextDouble(), _prg.NextDouble);

			CheckMax<double>(a => random.NextDouble(), _prg.NextDouble, 0);
		}

		[Fact]
		public void EventsTest()
		{
			EventsTestsShared.EventsTests<IPRG>(
				_prg,
				(G) =>
				{
					byte[] bytes = new byte[10];
					G.NextBytes(bytes);

					G.Next();
					G.Next(10);
					G.Next(10, 20);

					G.NextLong();
					G.NextLong(10);
					G.NextLong(10, 20);

					G.NextDouble();
					G.NextDouble(10);
					G.NextDouble(10, 20);
				},
				new Dictionary<Primitive, int> {
					{ Primitive.PRG, 10 }
				},
				new Dictionary<Primitive, int> {
					{ Primitive.PRG, 10 }
				}
			);
		}

		/// <summary>
		/// Helper function that verifies that PRG range methods indeed return
		/// values within requested ranges
		/// </summary>
		/// <param name="nextRand">Delegate that generates random number using C# built-in Random</param>
		/// <param name="nextPrg">Delegate that generate sample from range using tested PRG</param>
		/// <typeparam name="T">Type of the sample (number)</typeparam>
		private void CheckRanges<T>(Func<int, int, T> nextRand, Func<T, T, T> nextPrg) where T : IComparable
		{
			for (int i = 0; i < _runs; i++)
			{
				var a = nextRand(Int32.MinValue, Int32.MaxValue);
				var b = nextRand(Int32.MinValue, Int32.MaxValue);

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

		/// <summary>
		/// Helper function that verifies that PRG range methods indeed return
		/// values within requested ranges (0 to max)
		/// </summary>
		/// <param name="nextRand">Delegate that generates random number using C# built-in Random</param>
		/// <param name="nextPrg">Delegate that generate sample from range using tested PRG</param>
		/// <typeparam name="T">Type of the sample (number)</typeparam>
		private void CheckMax<T>(Func<int, T> nextRand, Func<T, T> nextPrg, T zero) where T : IComparable
		{
			for (int i = 0; i < _runs; i++)
			{
				var a = nextRand(Int32.MaxValue);

				var result = nextPrg(a);
				Assert.InRange(result, zero, a);
			}
		}
	}
}
