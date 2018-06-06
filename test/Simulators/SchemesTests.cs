using System;
using Xunit;
using Simulation;
using System.Linq;
using ORESchemes.Shared;
using DataStructures.BPlusTree;
using System.Collections.Generic;
using Simulation.PureSchemes;

namespace Test.Simulators
{
	[Trait("Category", "Unit")]
	public class SchemesTests
	{
		[Fact]
		public void SimulatorTest()
		{
			var max = 10000;
			var dataset =
				Enumerable
					.Range(-max, max)
					.ToList();

			var simulator = new Simulator<long>(dataset, new NoEncryptionScheme());
			var report = simulator.Simulate();

			Assert.NotEqual(0, report.OperationsNumber);
			Assert.NotEqual(new TimeSpan(0).Ticks, report.ObservedTime.Ticks);
		}
	}
}
