using System;
using Xunit;
using Simulation;
using System.Linq;
using ORESchemes.Shared;
using DataStructures.BPlusTree;
using System.Collections.Generic;
using Simulation.BPlusTree;

namespace Test.Simulators
{
	[Trait("Category", "Unit")]
	public class BPlusTreeTests
	{
		[Theory]
		[InlineData(QueriesType.Exact)]
		[InlineData(QueriesType.Range)]
		[InlineData(QueriesType.Update)]
		[InlineData(QueriesType.Delete)]
		public void SimulatorTest(QueriesType type)
		{
			var max = 10000;
			var inputs = new Inputs<string>
			{
				Type = type,
				Dataset =
					Enumerable
						.Range(1, max)
						.Select(val => new Record<string>(val, val.ToString()))
						.ToList(),
				ExactQueries =
					Enumerable
						.Range(1, max / 5)
						.Select(val => new ExactQuery(val * 4))
						.ToList(),
				RangeQueries =
					Enumerable
						.Range(1, max / 5)
						.Select(val => new RangeQuery(val * 3, val * 4))
						.ToList(),
				UpdateQueries =
					Enumerable
						.Range(1, max / 5)
						.Select(val => new UpdateQuery<string>(val * 4, (val * 4 + max).ToString()))
						.ToList(),
				DeleteQueries =
					Enumerable
						.Range(1, max / 5)
						.Select(val => new DeleteQuery(val * 4))
						.ToList(),
				CacheSize = 10
			};

			var options = new Options<long>(
				new NoEncryptionScheme(),
				3
			);

			var simulator = new Simulator<string, long>(inputs, options);
			var report = simulator.Simulate();

			Assert.Equal(type, report.QueriesType);

			var subreports = new List<Report.SubReport> {
				report.Construction,
				report.Queries
			};

			subreports.ForEach(
				subreport =>
				{
					Assert.NotEqual(0, report.Construction.IOs);
					Assert.NotEqual(0, report.Construction.SchemeOperations);
					Assert.NotEqual(new TimeSpan(0).Ticks, report.Construction.ObservedTime.Ticks);
					// CPU can be zero due to rounding and inacuracy
				}
			);

			var descriptions = new List<string> {
				report.ToString(),
				report.ToConciseString()
			};

			foreach (var description in descriptions)
			{
				foreach (var subreport in subreports)
				{
					Assert.Contains(subreport.IOs.ToString(), description);
					Assert.Contains(subreport.AvgIOs.ToString(), description);
					Assert.Contains(subreport.AvgSchemeOperations.ToString(), description);
					Assert.Contains(subreport.SchemeOperations.ToString(), description);
				}
			}
		}
	}
}
