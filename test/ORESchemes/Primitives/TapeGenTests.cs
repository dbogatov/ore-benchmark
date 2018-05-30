using System;
using System.Collections.Generic;
using System.Linq;
using ORESchemes.Shared.Primitives;
using Xunit;

namespace Test.ORESchemes.Primitives
{
	[Trait("Category", "Unit")]
	public class TapeGenTests
	{
		private const int SEED = 123456;
		private const int RUNS = 1000;

		[Fact]
		public void DifferentSeedsTest()
		{
			byte[] seedOne = new byte[256 / 8];
			byte[] seedTwo = new byte[256 / 8];

			byte[] keyOne = new byte[256 / 8];
			byte[] keyTwo = new byte[256 / 8];

			Random random = new Random(SEED);

			random.NextBytes(seedOne);
			random.NextBytes(seedTwo);
			random.NextBytes(keyOne);
			random.NextBytes(keyTwo);

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
	}
}
