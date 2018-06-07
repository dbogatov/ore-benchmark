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
	public class SchemesTests
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

			var simulator = new Simulator<Ciphertext>(dataset, new LewiOREScheme(16, entropy));
			var report = simulator.Simulate();

			var subreports = new List<Report.Subreport> { report.Encryptions, report.Decryptions, report.Comparisons };

			foreach (var subreport in subreports)
			{
				Assert.NotEqual(0, subreport.OperationsNumber);
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
					Assert.Contains(subreport.OperationsNumber.ToString(), description);
					Assert.Contains(subreport.ObservedTime.TotalMilliseconds.ToString(), description);
					Assert.Contains(subreport.CPUTime.TotalMilliseconds.ToString(), description);
				}
			}
		}
	}
}
