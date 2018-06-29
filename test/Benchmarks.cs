using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess;
using ORESchemes.Shared;
using Xunit;

namespace Test
{
	[Trait("Category", "Unit")]
	public class Benchmark
	{
		[Theory]
		[InlineData("Primitives")]
		[InlineData("Schemes")]
		public void DryJob(string @namespace)
		{
			var summary = BenchmarkSwitcher.FromTypes(
				new[] {
					typeof(global::Benchmark.Schemes.Benchmark<OPECipher, BytesKey>),
					typeof(global::Benchmark.Schemes.Benchmark<global::ORESchemes.PracticalORE.Ciphertext, BytesKey>),
					typeof(global::Benchmark.Schemes.Benchmark<global::ORESchemes.LewiORE.Ciphertext, global::ORESchemes.LewiORE.Key>),
					typeof(global::Benchmark.Schemes.Benchmark<global::ORESchemes.FHOPE.Ciphertext, global::ORESchemes.FHOPE.State>),
					typeof(global::Benchmark.Primitives.Benchmark)
				}
			).Run(new[] { $"--namespace=Benchmark.{@namespace}", "--join" }, new CustomConfig());
		}

		private class CustomConfig : ManualConfig
		{
			public CustomConfig() => Add(Job.Dry.With(InProcessToolchain.Instance));
		}
	}
}
