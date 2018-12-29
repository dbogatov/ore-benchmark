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
					dataset: "4, four\n5, five",
					queryset: "4, 5",
					datasetSize: 5,
					querysetSize: 5,
					pageSize: 5,
					protocol: global::ORESchemes.Shared.ORESchemes.NoEncryption,
					random: new Random()
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
					dataset: "4, four\n5",
					queryset: "4, 5",
					datasetSize: 5,
					querysetSize: 5,
					pageSize: 5,
					protocol: global::ORESchemes.Shared.ORESchemes.NoEncryption,
					random: new Random()
				)
			);
		}

		[Fact]
		public void SimulationParsingMalformedSet()
		{
			Assert.Throws<SingleSimulation.MalformedSetException>(
				() =>
				new SingleSimulation(
					dataset: "4, four\n5, five",
					queryset: "4",
					datasetSize: 5,
					querysetSize: 5,
					pageSize: 5,
					protocol: global::ORESchemes.Shared.ORESchemes.NoEncryption,
					random: new Random()
				)
			);
		}

		[Fact]
		public void SimulationDefault()
		{
			var simulation =
				new SingleSimulation(
					dataset: "",
					queryset: "",
					datasetSize: 5,
					querysetSize: 5,
					pageSize: 5,
					protocol: global::ORESchemes.Shared.ORESchemes.NoEncryption,
					random: new Random()
				);

			Assert.NotEmpty(simulation.Dataset);
			Assert.NotEmpty(simulation.Queryset);
		}
	}
}
