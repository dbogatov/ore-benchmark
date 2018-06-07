using System;
using System.Collections.Generic;
using System.Linq;
using ORESchemes.Shared.Primitives;
using ORESchemes.Shared.Primitives.TapeGen;
using Xunit;

namespace Test.ORESchemes.Primitives.LFPRF
{
	[Trait("Category", "Unit")]
	public class TapeGenTests
	{
		private const int SEED = 123456;
		private const int RUNS = 1000;

		private byte[] seedOne = new byte[256 / 8];
		private byte[] seedTwo = new byte[256 / 8];
		private byte[] keyOne = new byte[256 / 8];
		private byte[] keyTwo = new byte[256 / 8];

		public TapeGenTests()
		{
			Random random = new Random(SEED);

			random.NextBytes(seedOne);
			random.NextBytes(seedTwo);
			random.NextBytes(keyOne);
			random.NextBytes(keyTwo);
		}

		[Fact]
		public void DifferentSeedsTest()
		{
			var tapes = new List<TapeGen>() {
				new TapeGen(keyOne, seedOne),
				new TapeGen(keyOne, seedTwo),
				new TapeGen(keyTwo, seedOne),
				new TapeGen(keyTwo, seedTwo)
			};

			for (int i = 0; i < RUNS; i++)
			{
				Assert.Equal(
					tapes.Count,
					tapes.Select(tape => tape.Next()).Distinct().Count()
				);
			}
		}

		[Fact]
		public void EventsTest()
		{
			EventsTestsShared.EventsTests<TapeGen>(
				new TapeGen(keyOne, seedOne),
				(T) =>
				{
					byte[] bytes = new byte[10];
					T.NextBytes(bytes);

					T.Next();
					T.Next(10);
					T.Next(10, 20);

					T.NextLong();
					T.NextLong(10);
					T.NextLong(10, 20);

					T.NextDouble();
					T.NextDouble(10);
					T.NextDouble(10, 20);
				},
				new Dictionary<Primitive, int> {
					{ Primitive.PRF, 1 },
					{ Primitive.PRG, 10 },
					{ Primitive.LFPRF, 10 }
				},
				new Dictionary<Primitive, int> {
					{ Primitive.LFPRF, 10 }
				}
			);
		}
	}
}
