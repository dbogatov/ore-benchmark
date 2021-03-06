using System;
using Xunit;
using System.Linq;
using Simulation.PureSchemes;
using Crypto.LewiWu;

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

			var simulator = new Simulator<Ciphertext, Key>(dataset, new Scheme(16, entropy));
			var report = (global::Simulation.PureSchemes.Report)simulator.Simulate();

			var subreports = report.Stages.Values;

			foreach (global::Simulation.PureSchemes.Report.SubReport subreport in subreports)
			{
				Assert.NotEqual(0, subreport.SchemeOperations);
				Assert.NotEqual(new TimeSpan(0).Ticks, subreport.ObservedTime.Ticks);
				Assert.NotEqual(0, subreport.PurePrimitiveOperations.Values.Sum());
				Assert.NotEqual(0, subreport.TotalPrimitiveOperations.Values.Sum());

				Assert.NotEqual(0, subreport.MaxCipherSize);
				Assert.NotEqual(0, subreport.MaxStateSize);
			}

			var description = report.ToString();

			foreach (var subreport in subreports)
			{
				Assert.Contains(subreport.SchemeOperations.ToString(), description);
			}
		}
	}
}
