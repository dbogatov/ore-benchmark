using BenchmarkDotNet.Running;
using Crypto.Shared;
using McMaster.Extensions.CommandLineUtils;
using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using System.Linq;
using BenchmarkDotNet.Exporters.Json;

namespace Benchmark
{
	[HelpOption("-?|-h|--help")]
	[Command(Name = "benchmark", Description = "Utility that runs benchmarks against schemes and primitives", ThrowOnUnexpectedArgument = true)]
	public class Program
	{
		/// <summary>
		/// Entry point
		/// </summary>
		public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

		[Option("--primitives", "If present, only primitives will be benchmarked.", CommandOptionType.NoValue)]
		public bool Primitives { get; } = false;

		[Option("--schemes", "If present, only schmes will be benchmarked.", CommandOptionType.NoValue)]
		public bool Schemes { get; } = false;

		private int OnExecute()
		{
			if (
				(Primitives && Schemes) ||
				(!Primitives && !Schemes)
			)
			{
				throw new ArgumentException($"One and only one of --primitives and --schemes must be set.");
			}

			var @namespace = "";

			if (Primitives)
			{
				@namespace = "Primitives";
			}
			if (Schemes)
			{
				@namespace = "Schemes";
			}

			BenchmarkSwitcher.FromTypes(
				new[] {
					typeof(Schemes.Benchmark<OPECipher, BytesKey>),
					typeof(Schemes.Benchmark<Crypto.CLWW.Ciphertext, BytesKey>),
					typeof(Schemes.Benchmark<Crypto.LewiWu.Ciphertext, Crypto.LewiWu.Key>),
					typeof(Schemes.Benchmark<Crypto.FHOPE.Ciphertext, Crypto.FHOPE.State>),
					typeof(Schemes.Benchmark<Crypto.CLOZ.Ciphertext, Crypto.CLOZ.Key>),
					typeof(Primitives.Benchmark)
				}
			).Run(new[] { $"--namespace=Benchmark.{@namespace}", "--join" }, new CustomConfig());

			return 0;
		}

		private class CustomConfig : ManualConfig
		{
			public CustomConfig()
			{
				Add(MemoryDiagnoser.Default);
				Add(JsonExporter.Full);
				Add(DefaultConfig.Instance.GetLoggers().ToArray()); // manual config has no loggers by default
				Add(DefaultConfig.Instance.GetExporters().ToArray()); // manual config has no exporters by default
				Add(DefaultConfig.Instance.GetColumnProviders().ToArray()); // manual config has no columns by default
			}
		}
	}
}
