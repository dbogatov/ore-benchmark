using System;
using Xunit;
using System.Linq;
using Simulation.PureSchemes;
using ORESchemes.LewiORE;

namespace Test.Simulators
{
	[Trait("Category", "Unit")]
	public class PureSchemes
	{
		[Fact]
		public void Simulator()
		{
			byte[] entropy = new byte[128 / 8];
			new Random(123456).NextBytes(entropy);

			var max = 1000;
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

			var description = report.ToString();

			foreach (var subreport in subreports)
			{
				Assert.Contains(subreport.SchemeOperations.ToString(), description);
			}
		}
	}
}
