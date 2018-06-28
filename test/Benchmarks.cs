using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess;
using ORESchemes.Shared;
using Xunit;

namespace Test
{
	[Trait("Category", "Unit")]
	public class BenchmarkTests
	{
		[Theory]
		[InlineData("Primitives")]
		[InlineData("Schemes")]
		public void DryJob(string @namespace)
		{
			var summary = BenchmarkSwitcher.FromTypes(
				new[] {
					typeof(Benchmark.Schemes.Benchmark<OPECipher, BytesKey>),
					typeof(Benchmark.Schemes.Benchmark<global::ORESchemes.PracticalORE.Ciphertext, BytesKey>),
					typeof(Benchmark.Schemes.Benchmark<global::ORESchemes.LewiORE.Ciphertext, global::ORESchemes.LewiORE.Key>),
					typeof(Benchmark.Schemes.Benchmark<global::ORESchemes.FHOPE.Ciphertext, global::ORESchemes.FHOPE.State>),
					typeof(Benchmark.Primitives.Benchmark)
				}
			).Run(new[] { $"--namespace=Benchmark.{@namespace}", "--join" }, new CustomConfig());
		}

		private class CustomConfig : ManualConfig
		{
			public CustomConfig() => Add(Job.Dry.With(InProcessToolchain.Instance));
		}
	}
}
