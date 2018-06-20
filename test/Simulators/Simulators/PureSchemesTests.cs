using System;
using Xunit;
using Simulation;
using System.Linq;
using ORESchemes.Shared;
using DataStructures.BPlusTree;
using System.Collections.Generic;
using Simulation.PureSchemes;
using ORESchemes.LewiORE;

namespace Test.Simulators
{
	[Trait("Category", "Unit")]
	public class PureSchemesTests
	{
		[Fact]
		public void SimulatorTest()
		{
			byte[] entropy = new byte[256 / 8];
			new Random(123456).NextBytes(entropy);

			var max = 100;
			var dataset =
				Enumerable
					.Range(-max, max)
					.ToList();

			var simulator = new Simulator<Ciphertext, Key>(dataset, new LewiOREScheme(16, entropy));
			var report = simulator.Simulate();

			var subreports = report.Stages.Values;

			foreach (var subreport in subreports)
			{
				Assert.NotEqual(0, subreport.SchemeOperations);
				Assert.NotEqual(new TimeSpan(0).Ticks, subreport.ObservedTime.Ticks);
				Assert.NotEqual(0, subreport.PurePrimitiveOperations.Values.Sum());
				Assert.NotEqual(0, subreport.TotalPrimitiveOperations.Values.Sum());
			}

			var descriptions = new List<string> {
				report.ToString(),
				report.ToConciseString()
			};

			foreach (var description in descriptions)
			{
				foreach (var subreport in subreports)
				{
					Assert.Contains(subreport.SchemeOperations.ToString(), description);
				}
			}
		}
	}
}
