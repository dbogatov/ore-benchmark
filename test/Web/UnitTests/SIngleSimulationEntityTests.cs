using System;
using Web.Models.Data.Entities;
using Xunit;

namespace Test.Web.UnitTests
{
	[Trait("Category", "Unit")]
	public class SingleSimulationEntity
	{
		[Fact]
		public void SimulationParsingGood()
		{
			var simulation =
				new SingleSimulation(
					"4, four\n5, five",
					"4, 5",
					5,
					5,
					new Random()
				);

			Assert.NotEmpty(simulation.Dataset);
			Assert.NotEmpty(simulation.Queryset);
		}

		[Fact]
		public void SimulationParsingMalformedData()
		{
			Assert.Throws<SingleSimulation.MalformedSetException>(
				() =>
				new SingleSimulation(
					"4, four\n5",
					"4, 5",
					5,
					5,
					new Random()
				)
			);
		}

		[Fact]
		public void SimulationParsingMalformedSet()
		{
			Assert.Throws<SingleSimulation.MalformedSetException>(
				() =>
				new SingleSimulation(
					"4, four\n5, five",
					"4",
					5,
					5,
					new Random()
				)
			);
		}

		[Fact]
		public void SimulationDefault()
		{
			var simulation =
				new SingleSimulation(
					"", "", 5, 5, new Random()
				);

			Assert.NotEmpty(simulation.Dataset);
			Assert.NotEmpty(simulation.Queryset);
		}
	}
}
