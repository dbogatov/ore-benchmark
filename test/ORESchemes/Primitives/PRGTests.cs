using System;
using Crypto.Shared.Primitives;
using Crypto.Shared;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using Crypto.Shared.Primitives.PRG;

namespace Test.Crypto.Primitives.PRG
{
	[Trait("Category", "Unit")]
	public class AESPRGGenerator : AbsPRG
	{
		protected override Dictionary<Primitive, int> _totalEvents
		{
			get => new Dictionary<Primitive, int> {
					{ Primitive.PRG, 10 },
					{ Primitive.AES, 10 }
				};
			set => throw new NotImplementedException();
		}
		protected override Dictionary<Primitive, int> _pureEvents
		{
			get => new Dictionary<Primitive, int> {
					{ Primitive.PRG, 10 }
				};
			set => throw new NotImplementedException();
		}

		public AESPRGGenerator() : base()
		{
			_prg = new AESPRG(_entropy);
			_anotherPrg = new AESPRG(_anotherEntropy);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void Factory(bool seed)
		{
			byte[] entropy = seed ? _entropy : null;

			var prg = new PRGFactory(entropy).GetPrimitive();

			Assert.NotNull(prg);
			Assert.IsType<AESPRG>(prg);
		}
	}

	[Trait("Category", "Unit")]
	public class AESPRGCachedGenerator : AbsPRG
	{
		protected override Dictionary<Primitive, int> _totalEvents
		{
			get => new Dictionary<Primitive, int> {
					{ Primitive.PRG, 10 },
					{ Primitive.AES, 5 }
				};
			set => throw new NotImplementedException();
		}
		protected override Dictionary<Primitive, int> _pureEvents
		{
			get => new Dictionary<Primitive, int> {
					{ Primitive.PRG, 10 }
				};
			set => throw new NotImplementedException();
		}

		public AESPRGCachedGenerator() : base()
		{
			_prg = new AESPRGCached(_entropy);
			_anotherPrg = new AESPRGCached(_anotherEntropy);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void Factory(bool seed)
		{
			byte[] entropy = seed ? _entropy : null;

			var prg = new PRGCachedFactory(entropy).GetPrimitive();

			Assert.NotNull(prg);
			Assert.IsType<AESPRGCached>(prg);
		}
	}

	[Trait("Category", "Unit")]
	public class DefaultRandomGenerator : AbsPRG
	{
		public DefaultRandomGenerator() : base()
		{
			_prg = new DefaultRandom(_entropy);
			_anotherPrg = new DefaultRandom(_anotherEntropy);
		}

		protected override Dictionary<Primitive, int> _totalEvents
		{
			get => new Dictionary<Primitive, int> {
					{ Primitive.PRG, 10 }
				};
			set => throw new NotImplementedException();
		}
		protected override Dictionary<Primitive, int> _pureEvents
		{
			get => new Dictionary<Primitive, int> {
					{ Primitive.PRG, 10 }
				};
			set => throw new NotImplementedException();
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void Factory(bool seed)
		{
			byte[] entropy = seed ? _entropy : null;

			var prg = new DefaultPRGFactory(entropy).GetPrimitive();

			Assert.NotNull(prg);
			Assert.IsType<DefaultRandom>(prg);
		}
	}

	public abstract class AbsPRG
	{
		protected const int _seed = 132456;
		protected readonly byte[] _entropy = new byte[128 / 8];
		protected readonly byte[] _anotherEntropy = new byte[128 / 8];

		protected const int _runs = 1000;

		protected IPRG _prg;
		protected IPRG _anotherPrg;

		protected abstract Dictionary<Primitive, int> _totalEvents { get; set; }
		protected abstract Dictionary<Primitive, int> _pureEvents { get; set; }

		public AbsPRG()
		{
			new Random(_seed).NextBytes(_entropy);
			new Random(_seed + 1).NextBytes(_anotherEntropy);
		}

		[Fact]
		public void NoExceptions()
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
		public void DifferentSizeRequests(int size)
		{
			byte[] bytes = new byte[size];
			_prg.NextBytes(bytes);

			Assert.False(bytes.All(b => b == 0x00));
		}

		[Fact]
		public void NoRepetitions()
		{
			var values = new HashSet<int>(_runs);
			for (int i = 0; i < _runs; i++)
			{
				values.Add(_prg.Next());
			}

			Assert.Equal(_runs, values.Count);
		}

		[Fact]
		public void DifferentSeeds()
		{
			var values = new HashSet<int>(_runs);
			for (int i = 0; i < _runs; i++)
			{
				values.Add(_prg.Next());
				values.Add(_anotherPrg.Next());
			}

			Assert.Equal(_runs * 2, values.Count);
		}

		[Theory]
		[InlineData(0, 1)]
		[InlineData(-5, 5)]
		[InlineData(0, 5)]
		[InlineData(0, int.MaxValue / 2 + 5)]
		[InlineData(int.MinValue, int.MaxValue)]
		public void Uniformity(int from, int to)
		{
			const double STDDEV = 0.003;
			const int RUNS = _runs * 100;
			long RANGE = (long)to - from;

			var values = new Dictionary<int, int>();
			for (int i = 0; i < RUNS; i++)
			{
				var value = _prg.Next(from, to);

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

			Assert.InRange(values.Values.StdDev(), 0, RUNS * STDDEV);
			Assert.InRange(values.Where(kvp => kvp.Key < 100).Select(kvp => kvp.Value).StdDev(), 0, RUNS * STDDEV);
			Assert.InRange(values.Where(kvp => kvp.Key > RUNS - 100).Select(kvp => kvp.Value).StdDev(), 0, RUNS * STDDEV);
			Assert.InRange(
				values.Where(kvp => kvp.Key > RUNS / 2 - 50 && kvp.Key < RUNS / 2 + 50).Select(kvp => kvp.Value).StdDev(),
				0, RUNS * STDDEV
			);
		}

		[Fact]
		public void NextDouble()
		{
			HashSet<double> set = new HashSet<double>();

			for (int i = 0; i < _runs; i++)
			{
				set.Add(_prg.NextDouble());
			}

			Assert.InRange(set.Count, _runs * 0.995, _runs * 1.005);
		}

		[Fact]
		public void RangesInt()
		{
			var random = new Random(_seed);
			CheckRanges<int>(random.Next, _prg.Next);

			CheckMax<int>(random.Next, _prg.Next, 0);
		}

		[Fact]
		public void RangesLong()
		{
			var random = new Random(_seed);
			CheckRanges<long>((a, b) => (long)(random.NextDouble() * Int64.MaxValue), _prg.NextLong);

			CheckMax<long>(a => (long)(random.NextDouble() * Int64.MaxValue), _prg.NextLong, 0);
		}

		[Fact]
		public void RangesDouble()
		{
			var random = new Random(_seed);
			CheckRanges<double>((a, b) => random.NextDouble(), _prg.NextDouble);

			CheckMax<double>(a => random.NextDouble(), _prg.NextDouble, 0);
		}
		
		[Fact]
		public void ZeroRange()
		{
			const int minMax = 5;
			
			Assert.Equal(minMax, _prg.Next(minMax, minMax));
			Assert.Equal(minMax, _prg.NextLong(minMax, minMax));
			Assert.Equal(minMax, _prg.NextDouble(minMax, minMax));
		}

		[Fact]
		public void Events()
		{
			EventsTestsShared.Events<IPRG>(
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
				_totalEvents,
				_pureEvents
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
