using System;
using Xunit;
using Simulation;
using System.Linq;
using OPESchemes;
using DataStructures.BPlusTree;
using System.Collections.Generic;

namespace Test
{
	public class SimulatorTests
	{
		[Theory]
		[InlineData(QueriesType.Exact)]
		[InlineData(QueriesType.Range)]
		[InlineData(QueriesType.Update)]
		[InlineData(QueriesType.Delete)]
		public void SimulatorTest(QueriesType type)
		{
			var max = 10000;
			var inputs = new Inputs<int, string>
			{
				Type = type,
				Dataset =
					Enumerable
						.Range(1, max)
						.Select(val => new Record<int, string>(val, val.ToString()))
						.ToList(),
				ExactQueries =
					Enumerable
						.Range(1, max / 5)
						.Select(val => new ExactQuery<int>(val * 4))
						.ToList(),
				RangeQueries =
					Enumerable
						.Range(1, max / 5)
						.Select(val => new RangeQuery<int>(val * 3, val * 4))
						.ToList(),
				UpdateQueries =
					Enumerable
						.Range(1, max / 5)
						.Select(val => new UpdateQuery<int, string>(val * 4, (val * 4 + max).ToString()))
						.ToList(),
				DeleteQueries =
					Enumerable
						.Range(1, max / 5)
						.Select(val => new DeleteQuery<int>(val * 4))
						.ToList()
			};

			var options = new Options<int, int>(
				OPESchemesFactoryIntToInt.GetScheme(OPESchemes.OPESchemes.NoEncryption),
				3
			);

			var simulator = new Simulator<int, string, int>(inputs, options);
			var report = simulator.Simulate();

			Assert.Equal(type, report.QueriesType);

			new List<Report.SubReport> {
				report.Construction,
				report.Queries
			}.ForEach(
				subreport =>
				{
					Assert.NotEqual(0, report.Construction.IOs);
					Assert.NotEqual(0, report.Construction.SchemeOperations);
					Assert.NotEqual(new TimeSpan(0).Ticks, report.Construction.ObservedTime.Ticks);
					Assert.NotEqual(new TimeSpan(0).Ticks, report.Construction.CPUTime.Ticks);
				}
			);
		}
	}
}
